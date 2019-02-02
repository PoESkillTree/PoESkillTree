using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class ConfigurationStatsViewModel : ExplicitlyRegisteredStatsViewModel<ConfigurationStatViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly HashSet<IStat> _pinnedStats = new HashSet<IStat>();

        private ConfigurationStatsViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        public static ConfigurationStatsViewModel Create(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new ConfigurationStatsViewModel(nodeFactory);
            vm.Initialize(new ExplicitlyRegisteredStatsObserver(observableCalculator));
            return vm;
        }

        public void AddPinned(IStat stat)
        {
            _pinnedStats.Add(stat);
            Add(null, stat);
        }

        protected override bool IsResponsibleFor(IStat stat)
            => !_pinnedStats.Contains(stat) &&
               (stat.ExplicitRegistrationType is ExplicitRegistrationType.UserSpecifiedValue);

        protected override ConfigurationStatViewModel CreateViewModel(ICalculationNode node, IStat stat)
            => ConfigurationStatViewModel.Create(_nodeFactory, stat);

        protected override IStat SelectStat(ConfigurationStatViewModel statVm)
            => statVm.Stat;
    }
}