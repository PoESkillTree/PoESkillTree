using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public delegate NodeValue? NodeValueAggregator(List<NodeValue?> values);

    /// <summary>
    /// Aggregator methods for the different <see cref="Form"/>s for <see cref="FormAggregatingValue"/> and
    /// <see cref="MultiPathFormAggregatingValue"/>.
    /// </summary>
    public static class NodeValueAggregators
    {
        public static NodeValue? CalculateTotalOverride(List<NodeValue?> values)
            => SelectSingleValue(values);

        public static NodeValue? CalculateMore(List<NodeValue?> values)
            => values.Product(v => 1 + v / 100);

        public static NodeValue? CalculateIncrease(List<NodeValue?> values)
            => values.Sum(v => v / 100);

        public static NodeValue? CalculateBaseAdd(List<NodeValue?> values)
            => values.Sum();

        public static NodeValue? CalculateBaseSet(List<NodeValue?> values)
            => SelectSingleValue(values);

        private static NodeValue? SelectSingleValue(List<NodeValue?> values)
        {
            NodeValue? result = null;
            var hasDistinctValues = false;
            foreach (var nullableValue in values)
            {
                if (nullableValue is NodeValue value)
                {
                    if (value.IsZero)
                        return new NodeValue(0);
                    
                    if (result is null)
                        result = value;
                    else if (!value.AlmostEquals(result.Value))
                        hasDistinctValues = true;
                }
            }

            if (hasDistinctValues)
                throw new NotSupportedException("Multiple modifiers with none having value 0 are not supported");

            return result;
        }
    }
}