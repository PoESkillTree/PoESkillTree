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

        private readonly HashSet<(ICalculationNode node, IStat stat)> _elements =
            new HashSet<(ICalculationNode, IStat)>();

        public ExplicitlyRegisteredStatsObserver(ObservableCalculator observableCalculator)
            => _observableCalculator = observableCalculator;

        public event Action<ICalculationNode, IStat> StatAdded;
        public event Action<ICalculationNode, IStat> StatRemoved;

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
                    Add(((ICalculationNode, IStat)) args.Element);
                    break;
                case CollectionChangeAction.Remove:
                    Remove(((ICalculationNode, IStat)) args.Element);
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

        private void Refresh()
        {
            foreach (var element in _elements.ToList())
                Remove(element);

            foreach (var element in _observableCalculator.ExplicitlyRegisteredStatsCollection)
                Add(element);
        }

        private void Add((ICalculationNode, IStat) element)
        {
            if (_elements.Add(element))
                StatAdded?.Invoke(element.Item1, element.Item2);
        }

        private void Remove((ICalculationNode, IStat) element)
        {
            if (_elements.Remove(element))
                StatRemoved?.Invoke(element.Item1, element.Item2);
        }
    }
}