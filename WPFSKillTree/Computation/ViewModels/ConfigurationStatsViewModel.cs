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
            vm._explicitlyRegisteredStats.StatAdded += vm.AddIfUserSpecified;
            vm._explicitlyRegisteredStats.StatRemoved += vm.RemoveIfNotPinned;
            vm._explicitlyRegisteredStats.Initialize(DispatcherScheduler.Current);
            return vm;
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

        private void AddIfUserSpecified(IStat stat)
        {
            if (stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue)
            {
                Add(stat);
            }
        }

        private void RemoveIfNotPinned(IStat stat)
        {
            if (!_pinnedStats.Contains(stat))
            {
                Remove(stat);
            }
        }

        private ConfigurationStatViewModel Add(IStat stat)
        {
            var existingStat = Stats.FirstOrDefault(s => s.Stat.Equals(stat));
            if (existingStat != null)
                return existingStat;

            var configStat = new ConfigurationStatViewModel(_nodeFactory, stat);
            Stats.Add(configStat);
            return configStat;
        }

        private void Remove(IStat stat)
        {
            var configStat = Stats.First(s => s.Stat.Equals(stat));
            Stats.Remove(configStat);
            configStat.Dispose();
        }
    }
}