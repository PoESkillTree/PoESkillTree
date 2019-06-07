using System;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Model;

namespace PoESkillTree.Computation.ViewModels
{
    public class IndependentResultStatsViewModel : ExplicitlyRegisteredStatsViewModel<ResultStatViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;
        private readonly ModifierNodeViewModelFactory _modifierNodeFactory;

        private IndependentResultStatsViewModel(
            CalculationNodeViewModelFactory nodeFactory, ModifierNodeViewModelFactory modifierNodeFactory)
        {
            _nodeFactory = nodeFactory;
            _modifierNodeFactory = modifierNodeFactory;
        }

        public static async Task<IndependentResultStatsViewModel> CreateAsync(
            ObservableCalculator observableCalculator,
            CalculationNodeViewModelFactory nodeFactory,
            ModifierNodeViewModelFactory modifierNodeFactory)
        {
            var vm = new IndependentResultStatsViewModel(nodeFactory, modifierNodeFactory);
            await vm.InitializeAsync(new ExplicitlyRegisteredStatsObserver(observableCalculator));
            return vm;
        }

        protected override bool IsResponsibleFor(IStat stat)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.IndependentResult;

        protected override ResultStatViewModel CreateViewModel(ICalculationNode node, IStat stat)
        {
            var nodeType = ((ExplicitRegistrationType.IndependentResult) stat.ExplicitRegistrationType).ResultType;
            return new ResultStatViewModel(_nodeFactory.CreateResult(stat, nodeType), _modifierNodeFactory,
                _ => throw new NotSupportedException("Can't remove IndependentResult stats"));
        }

        protected override IStat SelectStat(ResultStatViewModel statVm)
            => statVm.Node.Stat;
    }
}