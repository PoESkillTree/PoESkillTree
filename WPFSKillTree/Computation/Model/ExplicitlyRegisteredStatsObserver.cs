using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace POESKillTree.Computation.Model
{
    public class ExplicitlyRegisteredStatsObserver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExplicitlyRegisteredStatsObserver));

        private readonly ObservableCalculator _observableCalculator;
        private readonly HashSet<IStat> _stats = new HashSet<IStat>();

        public ExplicitlyRegisteredStatsObserver(ObservableCalculator observableCalculator)
            => _observableCalculator = observableCalculator;

        public event Action<IStat> StatAdded;
        public event Action<IStat> StatRemoved;

        public void Initialize(IScheduler observeScheduler)
        {
            Refresh();
            _observableCalculator.ObserveExplicitlyRegisteredStats()
                .ObserveOn(observeScheduler)
                .Subscribe(OnNext, OnError);
        }

        private void OnNext(CollectionChangeEventArgs args)
        {
            switch (args.Action)
            {
                case CollectionChangeAction.Add:
                    Add(args.Element);
                    break;
                case CollectionChangeAction.Remove:
                    Remove(args.Element);
                    break;
                case CollectionChangeAction.Refresh:
                    Refresh();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void OnError(Exception exception)
            => Log.Error("ObserveExplicitlyRegisteredStats failed", exception);

        private void Add(object element)
        {
            var (_, stat) = ((ICalculationNode, IStat)) element;
            Add(stat);
        }

        private void Remove(object element)
        {
            var (_, stat) = ((ICalculationNode, IStat)) element;
            Remove(stat);
        }

        private void Refresh()
        {
            foreach (var stat in _stats.ToList())
            {
                Remove(stat);
            }

            foreach (var (_, stat) in _observableCalculator.ExplicitlyRegisteredStatsCollection)
            {
                Add(stat);
            }
        }

        private void Add(IStat stat)
        {
            if (_stats.Add(stat))
            {
                StatAdded?.Invoke(stat);
            }
        }

        private void Remove(IStat stat)
        {
            if (_stats.Remove(stat))
            {
                StatRemoved?.Invoke(stat);
            }
        }
    }
}