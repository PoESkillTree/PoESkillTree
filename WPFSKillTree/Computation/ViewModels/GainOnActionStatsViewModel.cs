using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Model;

namespace PoESkillTree.Computation.ViewModels
{
    public class GainOnActionStatsViewModel : ExplicitlyRegisteredStatsViewModel<GainOnActionStatViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private GainOnActionStatsViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        public static async Task<GainOnActionStatsViewModel> CreateAsync(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new GainOnActionStatsViewModel(nodeFactory);
            await vm.InitializeAsync(new ExplicitlyRegisteredStatsObserver(observableCalculator));
            return vm;
        }

        protected override bool IsResponsibleFor(IStat stat)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.GainOnAction;

        protected override GainOnActionStatViewModel CreateViewModel(ICalculationNode node, IStat stat)
            => new GainOnActionStatViewModel(_nodeFactory.CreateResult(stat, node));

        protected override IStat SelectStat(GainOnActionStatViewModel statVm)
            => statVm.Node.Stat;
    }
}