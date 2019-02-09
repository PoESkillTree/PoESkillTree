using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class GainOnActionStatsViewModel : ExplicitlyRegisteredStatsViewModel<GainOnActionStatViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private GainOnActionStatsViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        public static GainOnActionStatsViewModel Create(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new GainOnActionStatsViewModel(nodeFactory);
            vm.Initialize(new ExplicitlyRegisteredStatsObserver(observableCalculator));
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