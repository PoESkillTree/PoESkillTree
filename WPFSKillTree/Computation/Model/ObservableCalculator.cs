using System;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

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

        public IObservable<NodeValue?> ObserveNode(IStat stat, NodeType nodeType)
        {
            return Observable.Create<NodeValue?>(o => Subscribe(o))
                .SubscribeOn(_calculationScheduler);

            Action Subscribe(IObserver<NodeValue?> observer)
            {
                var node = _calculator.NodeRepository.GetNode(stat, nodeType);
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
    }
}