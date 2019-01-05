using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
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

        public ConfigurationStatsViewModel(ObservableCalculator observableCalculator)
            => _observableCalculator = observableCalculator;

        public ObservableCollection<ConfigurationStatViewModel> Stats { get; } =
            new ObservableCollection<ConfigurationStatViewModel>();

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
            var (node, stat) = ((ICalculationNode, IStat)) element;
            Add(node, stat);
        }

        private void Remove(object element)
        {
            var (_, stat) = ((ICalculationNode, IStat)) element;
            Remove(stat);
        }

        private void Refresh()
        {
            var configStats = Stats.ToList();
            Stats.Clear();
            configStats.ForEach(s => s.Dispose());

            foreach (var (node, stat) in _observableCalculator.ExplicitlyRegisteredStatsCollection)
            {
                Add(node, stat);
            }
        }

        private void Add(ICalculationNode node, IStat stat)
        {
            if (!(stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue))
                return;
            var configStat = new ConfigurationStatViewModel(stat);
            configStat.Observe(_observableCalculator, node);
            Stats.Add(configStat);
        }

        private void Remove(IStat stat)
        {
            if (!(stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue))
                return;
            var configStat = Stats.First(s => s.Stat.Equals(stat));
            Stats.Remove(configStat);
            configStat.Dispose();
        }
    }
}