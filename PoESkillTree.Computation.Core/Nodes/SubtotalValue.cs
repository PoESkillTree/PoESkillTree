using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.Subtotal"/>.
    /// </summary>
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
            if (uncapped is null)
                return null;

            var uncappedV = uncapped.Value;
            if (uncappedV.AlmostEquals(0))
                return new NodeValue(0);

            var min = _stat.Minimum is null ? null : context.GetValue(_stat.Minimum);
            var max = _stat.Maximum is null ? null : context.GetValue(_stat.Maximum);
            return Min(Max(uncappedV, min), max);
        }

        private static NodeValue Min(NodeValue left, NodeValue? right) =>
            Combine(left, right, Math.Min);

        private static NodeValue Max(NodeValue left, NodeValue? right) =>
            Combine(left, right, Math.Max);

        private static NodeValue Combine(NodeValue left, NodeValue? right, Func<double, double, double> operation)
        {
            if (!right.HasValue)
            {
                return left;
            }

            return NodeValue.Combine(left, right.Value, operation);
        }
    }
}