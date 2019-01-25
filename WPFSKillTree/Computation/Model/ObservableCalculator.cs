using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Computation.Model
{
    public class ObservableCalculator : IObservingCalculator
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

        private IObservable<NodeValue?> ObserveNode(Func<ICalculationNode> nodeFunc)
        {
            return Observable.Create<NodeValue?>(Subscribe)
                .SubscribeOn(_calculationScheduler);

            Action Subscribe(IObserver<NodeValue?> observer)
            {
                var node = nodeFunc();
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
                .Do(args => Log.Info($"ExplicitlyRegisteredStats: received {args.Action} {args.Element}"));
        }

        public IReadOnlyCollection<(ICalculationNode node, IStat stat)> ExplicitlyRegisteredStatsCollection
            => _calculator.ExplicitlyRegisteredStats;

        public Task ForEachUpdateCalculatorAsync(IObservable<CalculatorUpdate> observable)
            => observable.ObserveOn(_calculationScheduler)
                .ForEachAsync(UpdateCalculator);

        public IDisposable SubscribeTo(IObservable<CalculatorUpdate> observable, Action<Exception> onError)
            => observable.ObserveOn(_calculationScheduler)
                .Subscribe(UpdateCalculator, onError);

        private void UpdateCalculator(CalculatorUpdate update)
        {
            try
            {
                _calculator.Update(update);
                Log.Info($"Added {update.AddedModifiers.Count} and removed {update.RemovedModifiers.Count} modifiers");
            }
            catch (Exception e)
            {
                Log.Error($"_calculator.Update({update}) failed", e);
                throw;
            }
        }

        public Task<NodeValue?> GetNodeValueAsync(ICalculationNode node)
            => _calculationScheduler.ScheduleAsync(() => node.Value);

        public Task<NodeValue?> GetNodeValueAsync(IStat stat, NodeType nodeType = NodeType.Total)
            => _calculationScheduler.ScheduleAsync(() => GetNode(stat, nodeType).Value);

        private ICalculationNode GetNode(IStat stat, NodeType nodeType)
            => _calculator.NodeRepository.GetNode(stat, nodeType);

        public Task<NodeValue?> GetNodeValueAsync(IStat stat, NodeType nodeType, PathDefinition path)
            => _calculationScheduler.ScheduleAsync(() => GetNode(stat, nodeType, path).Value);

        private ICalculationNode GetNode(IStat stat, NodeType nodeType, PathDefinition path)
            => _calculator.NodeRepository.GetNode(stat, nodeType, path);

        public async Task<IEnumerable<(ICalculationNode node, Modifier modifier)>> GetFormNodeCollectionAsync(
            IStat stat, Form form, PathDefinition path)
            => await _calculator.NodeRepository.GetFormNodeCollection(stat, form, path)
                .ToObservable().SubscribeOn(_calculationScheduler)
                .ToList().SingleAsync();

        public async Task<IEnumerable<PathDefinition>> GetPathsAsync(IStat stat)
            => await _calculator.NodeRepository.GetPaths(stat)
                .ToObservable().SubscribeOn(_calculationScheduler)
                .ToList().SingleAsync();

        public IDisposable PeriodicallyRemoveUnusedNodes(Action<Exception> onError)
            => Observable.Interval(TimeSpan.FromMilliseconds(200))
                .ObserveOn(_calculationScheduler)
                .Subscribe(_ => _calculator.RemoveUnusedNodes(), onError);
    }
}