using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Core.Nodes
{
    public delegate NodeValue? NodeValueAggregator(IEnumerable<NodeValue?> values);

    /// <summary>
    /// Aggregator methods for the different <see cref="Form"/>s for <see cref="FormAggregatingValue"/> and
    /// <see cref="MultiPathFormAggregatingValue"/>.
    /// </summary>
    public static class NodeValueAggregators
    {
        public static NodeValue? CalculateTotalOverride(IEnumerable<NodeValue?> values)
        {
            var enumerated = values.Distinct().EnumerateWhereNotNull();
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
            values.Any(v => v == 0)
                ? new NodeValue(0)
                : throw new NotSupportedException(
                    "Multiple TotalOverride modifiers with none having value 0 are not supported");

        public static NodeValue? CalculateMore(IEnumerable<NodeValue?> values) =>
            values.SelectOnValues(v => 1 + v / 100).Product();

        public static NodeValue? CalculateIncrease(IEnumerable<NodeValue?> values) =>
            values.SelectOnValues(v => v / 100).Sum();

        public static NodeValue? CalculateBaseAdd(IEnumerable<NodeValue?> values) =>
            values.Sum();

        public static NodeValue? CalculateBaseSet(IEnumerable<NodeValue?> values)
        {
            var enumerated = values.EnumerateWhereNotNull();
            if (enumerated.Count(v => !v.Minimum.AlmostEquals(0, 1e-10)) > 1
                || enumerated.Count(v => !v.Maximum.AlmostEquals(0, 1e-10)) > 1)
            {
                throw new NotSupportedException("Multiple modifiers to BaseSet are not supported");
            }

            return enumerated.Any() ? enumerated.Sum() : (NodeValue?) null;
        }

        private static IReadOnlyList<NodeValue> EnumerateWhereNotNull(this IEnumerable<NodeValue?> values) =>
            values.OfType<NodeValue>().ToList();
    }
}