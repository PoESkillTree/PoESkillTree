using System.Reactive.Concurrency;
using PoESkillTree.Computation.Common;
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
            node.Observe(_calculator, _observerScheduler);
            return node;
        }

        public ConfigurationNodeViewModel CreateConfiguration(IStat stat)
        {
            var node = new ConfigurationNodeViewModel(stat);
            node.SubscribeCalculator(_calculator);
            return node;
        }
    }
}