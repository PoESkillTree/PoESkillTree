using System.Reactive.Concurrency;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Model;

namespace PoESkillTree.Computation.ViewModels
{
    public class CalculationNodeViewModelFactory
    {
        private readonly ModifierNodeViewModelFactory _modifierNodeFactory;
        private readonly ObservableCalculator _calculator;
        private readonly IScheduler _observerScheduler;

        public CalculationNodeViewModelFactory(
            ModifierNodeViewModelFactory modifierNodeFactory,
            ObservableCalculator calculator, IScheduler observerScheduler)
            => (_modifierNodeFactory, _calculator, _observerScheduler) =
                (modifierNodeFactory, calculator, observerScheduler);

        public ResultNodeViewModel CreateResult(IStat stat, NodeType nodeType = NodeType.Total)
        {
            var node = new ResultNodeViewModel(_modifierNodeFactory, stat, nodeType);
            node.Observe(_calculator.ObserveNode(stat, nodeType), _observerScheduler);
            return node;
        }

        public ResultNodeViewModel CreateResult(IStat stat, ICalculationNode calculationNode)
        {
            var node = new ResultNodeViewModel(_modifierNodeFactory, stat);
            node.Observe(_calculator.ObserveNode(calculationNode), _observerScheduler);
            return node;
        }

        public ConfigurationNodeViewModel CreateConfiguration(IStat stat, NodeValue? defaultValue = null)
        {
            var node = new ConfigurationNodeViewModel(stat, defaultValue);
            node.SubscribeCalculator(_calculator);
            node.ResetValue();
            return node;
        }
    }
}