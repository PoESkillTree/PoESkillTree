using System.Threading.Tasks;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;

namespace PoESkillTree.Computation.ViewModels
{
    public class IndependentResultStatsViewModel : ExplicitlyRegisteredStatsViewModel<ResultNodeViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private IndependentResultStatsViewModel(CalculationNodeViewModelFactory nodeFactory)
        {
            _nodeFactory = nodeFactory;
        }

        public static async Task<IndependentResultStatsViewModel> CreateAsync(
            ObservableCalculator observableCalculator,
            CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new IndependentResultStatsViewModel(nodeFactory);
            await vm.InitializeAsync(new ExplicitlyRegisteredStatsObserver(observableCalculator));
            return vm;
        }

        protected override bool IsResponsibleFor(IStat stat)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.IndependentResult;

        protected override ResultNodeViewModel CreateViewModel(ICalculationNode node, IStat stat)
        {
            var nodeType = ((ExplicitRegistrationType.IndependentResult) stat.ExplicitRegistrationType).ResultType;
            return _nodeFactory.CreateResult(stat, nodeType);
        }

        protected override IStat SelectStat(ResultNodeViewModel statVm)
            => statVm.Stat;
    }
}