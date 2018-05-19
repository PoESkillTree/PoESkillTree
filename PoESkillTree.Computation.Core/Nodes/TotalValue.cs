using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.Total"/>.
    /// </summary>
    public class TotalValue : IValue
    {
        private readonly IStat _stat;

        public TotalValue(IStat stat)
        {
            _stat = stat;
        }

        public NodeValue? Calculate(IValueCalculationContext context) =>
            context.GetValue(_stat, NodeType.TotalOverride) ?? context.GetValue(_stat, NodeType.Subtotal);
    }
}