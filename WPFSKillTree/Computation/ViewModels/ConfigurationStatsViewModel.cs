using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;

namespace PoESkillTree.Computation.ViewModels
{
    public class ConfigurationStatsViewModel : ExplicitlyRegisteredStatsViewModel<ConfigurationStatViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly HashSet<IStat> _pinnedStats = new HashSet<IStat>();

        private ConfigurationStatsViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        public static async Task<ConfigurationStatsViewModel> CreateAsync(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new ConfigurationStatsViewModel(nodeFactory);
            await vm.InitializeAsync(new ExplicitlyRegisteredStatsObserver(observableCalculator));
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