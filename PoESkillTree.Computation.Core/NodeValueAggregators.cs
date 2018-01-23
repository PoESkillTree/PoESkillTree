using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public static class NodeValueAggregators
    {
        // For both BaseOverride and TotalOverride
        public static NodeValue? CalculateOverride(IEnumerable<NodeValue?> values)
        {
            var enumerated = values.EnumerateWhereNotNull();
            switch (enumerated.Count)
            {
                case 0:
                    return null;
                case 1:
                    return enumerated[0];
                default:
                    return CalculateOverrideFromMany(enumerated);
            }
        }

        private static NodeValue? CalculateOverrideFromMany(IEnumerable<NodeValue> values) =>
            values.Any(v => v.AlmostEquals(0))
                ? new NodeValue(0)
                : throw new NotSupportedException(
                    "Multiple modifiers to BaseOverride or TotalOverride with none having value 0 are not supported");

        public static NodeValue? CalculateMore(IEnumerable<NodeValue?> values) => 
            values.AggregateOrNull(vs => vs.Select(v => 1 + v / 100).Product());

        public static NodeValue? CalculateIncrease(IEnumerable<NodeValue?> values) => 
            values.AggregateOrNull(vs => 1 + vs.Select(v => v / 100).Sum());

        public static NodeValue? CalculateBaseAdd(IEnumerable<NodeValue?> values) => 
            values.AggregateOrNull(Sum);

        public static NodeValue? CalculateBaseSet(IEnumerable<NodeValue?> values)
        {
            var enumerated = values.EnumerateWhereNotNull();
            if (enumerated.Count(v => !v.Minimum.AlmostEquals(0, 1e-10)) > 1
                || enumerated.Count(v => !v.Maximum.AlmostEquals(0, 1e-10)) > 1)
            {
                throw new NotSupportedException("Multiple modifiers to BaseSet are not supported");
            }

            return enumerated.Any() ? enumerated.Sum() : new NodeValue(0);
        }

        // This only has BaseSet and BaseAdd as children and is not identical to the Base node.
        // The Base node is an OverwritableNode with this and BaseOverride as children.
        // Base is never null because BaseSet is never null.
        public static NodeValue? CalculateBase(IEnumerable<NodeValue?> values) => 
            values.AggregateOrNull(Sum);

        // UncappedSubtotal has Base, Increase and More as children. It is never null because Base is never null.
        public static NodeValue? CalculateUncappedSubtotal(IEnumerable<NodeValue?> values) => 
            values.AggregateOrNull(Product);

        private static NodeValue Sum(this IEnumerable<NodeValue> values) =>
            values.Aggregate((l, r) => l + r);

        private static NodeValue Product(this IEnumerable<NodeValue> values) =>
            values.Aggregate((l, r) => l * r);

        private static NodeValue? AggregateOrNull(
            this IEnumerable<NodeValue?> values, Func<IEnumerable<NodeValue>, NodeValue> aggregator)
        {
            var enumerated = values.EnumerateWhereNotNull();
            if (enumerated.Any())
            {
                return aggregator(enumerated);
            }
            return null;
        }

        private static IReadOnlyList<NodeValue> EnumerateWhereNotNull(this IEnumerable<NodeValue?> values) => 
            values.OfType<NodeValue>().ToList();
    }
}