using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Model
{
    public class ObservableCalculator : IObservingCalculator
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ICalculator _calculator;
        private readonly IScheduler _calculationScheduler;

        private readonly Lazy<IObservable<CollectionChangedEventArgs<(ICalculationNode, IStat)>>>
            _explicitlyRegisteredStatsObservable;

        private readonly Lazy<Subject<IObservable<CalculatorUpdate>>> _updateSubject;

        public ObservableCalculator(ICalculator calculator, IScheduler calculationScheduler)
        {
            (_calculator, _calculationScheduler) = (calculator, calculationScheduler);
            _explicitlyRegisteredStatsObservable =
                new Lazy<IObservable<CollectionChangedEventArgs<(ICalculationNode, IStat)>>>(
                    CreateExplicitlyRegisteredStatsObservable);
            _updateSubject = new Lazy<Subject<IObservable<CalculatorUpdate>>>(CreateUpdateSubject);
        }

        public IObservable<NodeValue?> ObserveNode(IStat stat, NodeType nodeType = NodeType.Total)
            => ObserveNode(() => GetNode(stat, nodeType));

        public IObservable<NodeValue?> ObserveNode(IStat stat, NodeType nodeType, ModifierSource modifierSource)
            => ObserveNode(() => GetNode(stat, nodeType, new PathDefinition(modifierSource)));

        public IObservable<NodeValue?> ObserveNode(ICalculationNode node)
            => ObserveNode(() => node);

        private IObservable<NodeValue?> ObserveNode(Func<ICalculationNode> nodeFunc)
        {
            return Observable.Create<NodeValue?>(Subscribe)
                .Throttle(TimeSpan.FromMilliseconds(20), _calculationScheduler)
                .SubscribeOn(_calculationScheduler);

            Action Subscribe(IObserver<NodeValue?> observer)
            {
                var node = nodeFunc();
                void ValueChanged(object? _, EventArgs __) => observer.OnNext(node.Value);
                node.ValueChanged += ValueChanged;
                observer.OnNext(node.Value);
                return () => node.ValueChanged -= ValueChanged;
            }
        }

        public IObservable<CollectionChangedEventArgs<(ICalculationNode node, IStat stat)>>
            ObserveExplicitlyRegisteredStats()
            => _explicitlyRegisteredStatsObservable.Value;

        private IObservable<CollectionChangedEventArgs<(ICalculationNode, IStat)>>
            CreateExplicitlyRegisteredStatsObservable()
        {
            var collection = _calculator.ExplicitlyRegisteredStats;
            return Observable.FromEventPattern<CollectionChangedEventHandler<(ICalculationNode, IStat)>,
                    CollectionChangedEventArgs<(ICalculationNode, IStat)>>(
                    h => collection.CollectionChanged += h,
                    h => collection.CollectionChanged -= h)
                .Select(p => p.EventArgs);
        }

        public Task<List<(ICalculationNode node, IStat stat)>> GetExplicitlyRegisteredStatsAsync()
            => _calculationScheduler.ScheduleAsync(() => _calculator.ExplicitlyRegisteredStats.ToList());

        public Task ForEachUpdateCalculatorAsync(IObservable<CalculatorUpdate> observable)
            => observable.ObserveOn(_calculationScheduler)
                .ForEachAsync(UpdateCalculator);

        public Task UpdateCalculatorAsync(Task<CalculatorUpdate> update) =>
            _calculationScheduler.ScheduleAsync(async () => UpdateCalculator(await update.ConfigureAwait(false)));

        public void SubscribeTo(IObservable<CalculatorUpdate> observable)
            => _updateSubject.Value.OnNext(observable);

        private void UpdateCalculator(CalculatorUpdate update)
        {
            try
            {
                _calculator.Update(update);
                Log.Info($"Added {update.AddedModifiers.Count} and removed {update.RemovedModifiers.Count} modifiers");
            }
            catch (Exception e)
            {
                Log.Error(e, $"_calculator.Update({update}) failed");
                throw;
            }
        }

        private Subject<IObservable<CalculatorUpdate>> CreateUpdateSubject()
        {
            var subject = new Subject<IObservable<CalculatorUpdate>>();
            subject.Merge()
                .Buffer(TimeSpan.FromMilliseconds(50))
                .Where(us => us.Any())
                .Select(us => us.Aggregate(CalculatorUpdate.Accumulate))
                .ObserveOn(_calculationScheduler)
                .Subscribe(UpdateCalculator, ex => Log.Error(ex, "Exception while observing calculator updates"));
            return subject;
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
            IStat stat, Form form, PathDefinition path) =>
            await _calculationScheduler.ScheduleAsync(() => _calculator.NodeRepository.GetFormNodeCollection(stat, form, path));

        public async Task<IEnumerable<PathDefinition>> GetPathsAsync(IStat stat) =>
            await _calculationScheduler.ScheduleAsync(() => _calculator.NodeRepository.GetPaths(stat));

        public IDisposable PeriodicallyRemoveUnusedNodes(Action<Exception> onError)
            => Observable.Interval(TimeSpan.FromMilliseconds(250), _calculationScheduler)
                .ObserveOn(_calculationScheduler)
                .Subscribe(_ => _calculator.RemoveUnusedNodes(), onError);
    }
}