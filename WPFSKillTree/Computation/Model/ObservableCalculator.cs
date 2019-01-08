using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Utils;

namespace POESKillTree.Computation.Model
{
    public class ObservableCalculator
    {
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
                .SubscribeOn(_calculationScheduler);
        }

        public IReadOnlyCollection<(ICalculationNode node, IStat stat)> ExplicitlyRegisteredStatsCollection
            => _calculator.ExplicitlyRegisteredStats;

        public IDisposable SubscribeCalculatorTo(IObservable<CalculatorUpdate> observable, Action<Exception> onError)
            => observable.ObserveOn(_calculationScheduler)
                .Subscribe(_calculator.Update, onError);

        public Task<NodeValue?> GetNodeValueAsync(IStat stat)
            => _calculationScheduler.ScheduleAsync(() => GetNode(stat).Value);

        public Task<ICalculationNode> GetNodeAsync(IStat stat)
            => _calculationScheduler.ScheduleAsync(() => GetNode(stat));

        private ICalculationNode GetNode(IStat stat, NodeType nodeType = NodeType.Total)
            => _calculator.NodeRepository.GetNode(stat, nodeType);
    }
}