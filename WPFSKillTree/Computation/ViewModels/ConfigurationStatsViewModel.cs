using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using PoESkillTree.Computation.Common;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatsViewModel
    {
        private readonly ExplicitlyRegisteredStatsObserver _explicitlyRegisteredStats;
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly HashSet<IStat> _pinnedStats = new HashSet<IStat>();

        private ConfigurationStatsViewModel(
            CalculationNodeViewModelFactory nodeFactory, ExplicitlyRegisteredStatsObserver explicitlyRegisteredStats)
        {
            _nodeFactory = nodeFactory;
            _explicitlyRegisteredStats = explicitlyRegisteredStats;
        }

        public static ConfigurationStatsViewModel Create(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var explicitlyRegisteredStats = new ExplicitlyRegisteredStatsObserver(observableCalculator);
            var vm = new ConfigurationStatsViewModel(nodeFactory, explicitlyRegisteredStats);
            vm.Initialize();
            return vm;
        }

        private void Initialize()
        {
            _explicitlyRegisteredStats.StatAdded += (_, stat) => DoIfResponsible(stat, s => Add(s));
            _explicitlyRegisteredStats.StatRemoved += (_, stat) => DoIfResponsible(stat, Remove);
            _explicitlyRegisteredStats.Initialize(DispatcherScheduler.Current);
        }

        public ObservableCollection<ConfigurationStatViewModel> Stats { get; } =
            new ObservableCollection<ConfigurationStatViewModel>();

        public void AddPinned(IStat stat)
        {
            _pinnedStats.Add(stat);
            Add(stat);
        }

        public void AddPinned(IStat stat, NodeValue? initialValue)
        {
            _pinnedStats.Add(stat);
            var configStat = Add(stat);
            configStat.Node.Value = initialValue;
        }

        private void DoIfResponsible(IStat stat, Action<IStat> action)
        {
            if (IsResponsibleFor(stat))
                action(stat);
        }

        private bool IsResponsibleFor(IStat stat)
            => !_pinnedStats.Contains(stat) &&
               (stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue);

        private ConfigurationStatViewModel Add(IStat stat)
        {
            if (TryGetConfigStat(stat, out var existingStat))
                return existingStat;

            var configStat = new ConfigurationStatViewModel(_nodeFactory, stat);
            Stats.Add(configStat);
            return configStat;
        }

        private void Remove(IStat stat)
        {
            if (!TryGetConfigStat(stat, out var configStat))
                return;

            Stats.Remove(configStat);
            configStat.Dispose();
        }

        private bool TryGetConfigStat(IStat stat, out ConfigurationStatViewModel configStat)
        {
            configStat = Stats.FirstOrDefault(s => s.Stat.Equals(stat));
            return configStat != null;
        }
    }
}