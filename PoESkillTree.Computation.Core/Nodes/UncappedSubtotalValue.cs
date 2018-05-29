using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.UncappedSubtotal"/>.
    /// </summary>
    public class UncappedSubtotalValue : IValue
    {
        private readonly IStat _stat;

        public UncappedSubtotalValue(IStat stat) => 
            _stat = stat;

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext) =>
            valueCalculationContext
                .GetValues(_stat, NodeType.PathTotal)
                .Sum();
    }
}