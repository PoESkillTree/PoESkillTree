using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public abstract class ExplicitlyRegisteredStatsViewModel<T>
        where T : IDisposable
    {
        protected void Initialize(ExplicitlyRegisteredStatsObserver explicitlyRegisteredStats)
        {
            explicitlyRegisteredStats.StatAdded +=
                (node, stat) => DoIfResponsible(stat, () => Add(node, stat));
            explicitlyRegisteredStats.StatRemoved +=
                (node, stat) => DoIfResponsible(stat, () => Remove(stat));
            explicitlyRegisteredStats.Initialize(DispatcherScheduler.Current);
        }

        public ObservableCollection<T> Stats { get; } = new ObservableCollection<T>();

        private void DoIfResponsible(IStat stat, Action action)
        {
            if (IsResponsibleFor(stat))
                action();
        }

        protected void Add(ICalculationNode node, IStat stat)
        {
            if (TryGetStatViewModel(stat, out _))
                return;

            var statVm = CreateViewModel(node, stat);
            Stats.Add(statVm);
        }

        private void Remove(IStat stat)
        {
            if (!TryGetStatViewModel(stat, out var statVm))
                return;

            Stats.Remove(statVm);
            statVm.Dispose();
        }

        private bool TryGetStatViewModel(IStat stat, out T statVm)
        {
            statVm = Stats.FirstOrDefault(s => SelectStat(s).Equals(stat));
            return statVm != null;
        }

        protected abstract bool IsResponsibleFor(IStat stat);

        protected abstract T CreateViewModel(ICalculationNode node, IStat stat);

        protected abstract IStat SelectStat(T statVm);
    }
}