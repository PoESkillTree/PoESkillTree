using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatsViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigurationStatsViewModel));

        private readonly ObservableCalculator _observableCalculator;
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly HashSet<IStat> _pinnedStats = new HashSet<IStat>();

        public ConfigurationStatsViewModel(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
            => (_observableCalculator, _nodeFactory) = (observableCalculator, nodeFactory);

        public ObservableCollection<ConfigurationStatViewModel> Stats { get; } =
            new ObservableCollection<ConfigurationStatViewModel>();

        public async Task AddPinnedAsync(IStat stat, bool initializeWithCurrentValue)
        {
            _pinnedStats.Add(stat);
            var configStat = Add(stat);
            if (initializeWithCurrentValue)
            {
                configStat.Node.Value = await _observableCalculator.GetNodeValueAsync(stat);
            }
        }

        public void Observe()
        {
            Refresh();
            _observableCalculator.ObserveExplicitlyRegisteredStats()
                .ObserveOnDispatcher()
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
            AddIfUserSpecified(stat);
        }

        private void Remove(object element)
        {
            var (_, stat) = ((ICalculationNode, IStat)) element;
            RemoveIfNotPinned(stat);
        }

        private void Refresh()
        {
            foreach (var stat in Stats.Select(vm => vm.Stat).ToList())
            {
                RemoveIfNotPinned(stat);
            }

            foreach (var (_, stat) in _observableCalculator.ExplicitlyRegisteredStatsCollection)
            {
                AddIfUserSpecified(stat);
            }
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