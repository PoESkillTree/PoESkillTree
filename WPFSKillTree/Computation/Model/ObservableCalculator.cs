using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Utils;

namespace POESKillTree.Computation.Model
{
    public class ObservableCalculator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ObservableCalculator));

        private readonly ICalculator _calculator;
        private readonly IScheduler _calculationScheduler;
        private readonly Lazy<IObservable<CollectionChangeEventArgs>> _explicitlyRegisteredStatsObservable;

        public ObservableCalculator(ICalculator calculator, IScheduler calculationScheduler)
        {
            (_calculator, _calculationScheduler) = (calculator, calculationScheduler);
            _explicitlyRegisteredStatsObservable =
                new Lazy<IObservable<CollectionChangeEventArgs>>(CreateExplicitlyRegisteredStatsObservable);
        }

        public IObservable<NodeValue?> ObserveNode(IStat stat, NodeType nodeType = NodeType.Total)
            => ObserveNode(() => GetNode(stat, nodeType));

        public IObservable<NodeValue?> ObserveNode(ICalculationNode node)
            => ObserveNode(() => node);

        private IObservable<NodeValue?> ObserveNode(Func<ICalculationNode> getNode)
        {
            return Observable.Create<NodeValue?>(o => Subscribe(o))
                .SubscribeOn(_calculationScheduler);

            Action Subscribe(IObserver<NodeValue?> observer)
            {
                var node = getNode();
                void ValueChanged(object _, EventArgs __) => observer.OnNext(node.Value);
                node.ValueChanged += ValueChanged;
                observer.OnNext(node.Value);
                return () => node.ValueChanged -= ValueChanged;
            }
        }

        public IObservable<CollectionChangeEventArgs> ObserveExplicitlyRegisteredStats()
            => _explicitlyRegisteredStatsObservable.Value;

        private IObservable<CollectionChangeEventArgs> CreateExplicitlyRegisteredStatsObservable()
        {
            var collection = _calculator.ExplicitlyRegisteredStats;
            return Observable.FromEventPattern<CollectionChangeEventHandler, CollectionChangeEventArgs>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p => p.EventArgs)
                .Do(args => Log.Info($"ExplicitlyRegisteredStats: received {args.Action} {args.Element}"))
                .SubscribeOn(_calculationScheduler);
        }

        public IReadOnlyCollection<(ICalculationNode node, IStat stat)> ExplicitlyRegisteredStatsCollection
            => _calculator.ExplicitlyRegisteredStats;

        public Task SubscribeCalculatorToAndAwaitCompletionAsync(IObservable<CalculatorUpdate> observable)
            => observable.ObserveOn(_calculationScheduler)
                .SubscribeAndAwaitCompletionAsync(UpdateCalculator);

        public IDisposable SubscribeCalculatorTo(IObservable<CalculatorUpdate> observable, Action<Exception> onError)
            => observable.ObserveOn(_calculationScheduler)
                .Subscribe(UpdateCalculator, onError);

        private void UpdateCalculator(CalculatorUpdate update)
        {
            _calculator.Update(update);
            Log.Info($"Added {update.AddedModifiers.Count} and removed {update.RemovedModifiers.Count} modifiers");
        }

        public Task<NodeValue?> GetNodeValueAsync(IStat stat)
            => _calculationScheduler.ScheduleAsync(() => GetNode(stat).Value);

        private ICalculationNode GetNode(IStat stat, NodeType nodeType = NodeType.Total)
            => _calculator.NodeRepository.GetNode(stat, nodeType);

        public IDisposable PeriodicallyRemoveUnusedNodes(TimeSpan period, Action<Exception> onError)
            => Observable.Interval(TimeSpan.FromMilliseconds(200))
                .ObserveOn(_calculationScheduler)
                .Subscribe(_ => _calculator.RemoveUnusedNodes(), onError);
    }
}