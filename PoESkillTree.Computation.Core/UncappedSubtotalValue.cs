using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public class UncappedSubtotalValue : IValue
    {
        private readonly IStat _stat;

        public UncappedSubtotalValue(IStat stat)
        {
            _stat = stat;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var @base = context.GetValue(_stat, NodeType.Base);
            if (@base == null)
            {
                return null;
            }

            var increase = context.GetValue(_stat, NodeType.Increase) ?? new NodeValue(1);
            var more = context.GetValue(_stat, NodeType.More) ?? new NodeValue(1);
            return @base * increase * more;
        }
    }
}