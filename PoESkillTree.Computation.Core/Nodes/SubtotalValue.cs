using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public class SubtotalValue : IValue
    {
        private readonly IStat _stat;

        public SubtotalValue(IStat stat)
        {
            _stat = stat;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var uncapped = context.GetValue(_stat, NodeType.UncappedSubtotal);
            if (uncapped is NodeValue v && v.AlmostEquals(0))
            {
                return new NodeValue(0);
            }

            var min = context.GetValue(_stat.Minimum);
            var max = context.GetValue(_stat.Maximum);
            return Min(Max(uncapped, min), max);
        }

        private static NodeValue? Min(NodeValue? left, NodeValue? right) =>
            Combine(left, right, Math.Min);

        private static NodeValue? Max(NodeValue? left, NodeValue? right) =>
            Combine(left, right, Math.Max);

        private static NodeValue? Combine(NodeValue? left, NodeValue? right, Func<double, double, double> operation)
        {
            if (!left.HasValue)
            {
                return right;
            }

            if (!right.HasValue)
            {
                return left;
            }

            return NodeValue.Combine(left.Value, right.Value, operation);
        }
    }
}