using System.Reactive.Concurrency;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace POESKillTree.Computation.ViewModels
{
    public class CalculationNodeViewModelFactory
    {
        private readonly ObservableCalculator _calculator;
        private readonly IScheduler _observerScheduler;

        public CalculationNodeViewModelFactory(ObservableCalculator calculator, IScheduler observerScheduler)
            => (_calculator, _observerScheduler) = (calculator, observerScheduler);

        public ResultNodeViewModel CreateResult(IStat stat, NodeType nodeType = NodeType.Total)
        {
            var node = new ResultNodeViewModel(stat, nodeType);
            node.Observe(_calculator.ObserveNode(stat, nodeType), _observerScheduler);
            return node;
        }

        public ResultNodeViewModel CreateResult(IStat stat, ICalculationNode calculationNode)
        {
            var node = new ResultNodeViewModel(stat);
            node.Observe(_calculator.ObserveNode(calculationNode), _observerScheduler);
            return node;
        }

        public async Task<ResultNodeViewModel> CreateConstantResultAsync(IStat stat, ICalculationNode calculationNode)
            => new ResultNodeViewModel(stat) { Value = await _calculator.GetNodeValueAsync(calculationNode) };

        public ConfigurationNodeViewModel CreateConfiguration(IStat stat)
        {
            var node = new ConfigurationNodeViewModel(stat);
            node.SubscribeCalculator(_calculator);
            return node;
        }
    }
}