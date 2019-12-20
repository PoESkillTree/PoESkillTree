using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;

namespace PoESkillTree.Computation.ViewModels
{
    public class PassiveTreeConnectionsViewModel : ExplicitlyRegisteredStatsViewModel<PassiveTreeConnectionViewModel>
    {
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        private PassiveTreeConnectionsViewModel(CalculationNodeViewModelFactory nodeFactory)
            => _nodeFactory = nodeFactory;

        public static async Task<PassiveTreeConnectionsViewModel> CreateAsync(
            ObservableCalculator observableCalculator, CalculationNodeViewModelFactory nodeFactory)
        {
            var vm = new PassiveTreeConnectionsViewModel(nodeFactory);
            await vm.InitializeAsync(new ExplicitlyRegisteredStatsObserver(observableCalculator));
            return vm;
        }

        public IEnumerable<ushort> GetConnectedNodes(IReadOnlyCollection<ushort> sourceNodes) => Stats
            .Where(c => c.Connected && sourceNodes.Contains(c.SourceNode))
            .Select(c => c.TargetNode)
            .Where(n => !sourceNodes.Contains(n));

        protected override bool IsResponsibleFor(IStat stat)
            => stat.ExplicitRegistrationType is ExplicitRegistrationType.PassiveTreeConnection;

        protected override PassiveTreeConnectionViewModel CreateViewModel(ICalculationNode? node, IStat stat)
            => new PassiveTreeConnectionViewModel(_nodeFactory.CreateResult(stat, node!));

        protected override IStat SelectStat(PassiveTreeConnectionViewModel statVm)
            => statVm.Node.Stat;
    }
}