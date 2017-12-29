using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    /// <summary>
    /// Contains utility methods related to <see cref="ValueBuilder"/>s.
    /// </summary>
    public static class ValueBuilderUtils
    {
        /// <summary>
        /// Returns a value converter multiplying values by <paramref name="stat"/>'s value.
        /// </summary>
        public static Func<ValueBuilder, ValueBuilder> PerStat(IStatBuilder stat) =>
            v => v * stat.Value;

        /// <summary>
        /// Returns a value converter multiplying values by <c>(stat.Value / divideBy).Floor</c>.
        /// </summary>
        public static Func<ValueBuilder, ValueBuilder> PerStat(IStatBuilder stat, ValueBuilder divideBy) =>
            v => v * (stat.Value / divideBy).Floor;

        /// <summary>
        /// Returns a value converter multiplying values by <c>(stat.Value / divideBy).Ceiling</c>.
        /// </summary>
        public static Func<ValueBuilder, ValueBuilder> PerStatCeiled(IStatBuilder stat, ValueBuilder divideBy) =>
            v => v * (stat.Value / divideBy).Ceiling;

        /// <summary>
        /// Returns a value converter dividing values by 100 and multiplying them by <paramref name="stat"/>'s value.
        /// </summary>
        public static Func<ValueBuilder, ValueBuilder> PercentOf(IStatBuilder stat) =>
            v => stat.Value * v.AsPercentage;

        /// <summary>
        /// Builds a function from <paramref name="points"/> by interpolating linearly between each two consecutive
        /// points. Returns a value equal to the function applied to <paramref name="xStat"/>'s value.
        /// <para>Expressed differently: If <paramref name="xStat"/>'s value is equal to the x value of a point, that 
        /// point's y value is returned. Otherwise the y value is calculated by linear interpolation between
        /// the nearest smaller and nearest bigger point.</para>
        /// </summary>
        /// <remarks>
        /// At least 2 points must be given and they must be ordered by x values.
        /// </remarks>
        public static ValueBuilder LinearScale(this IValueBuilders valueFactory,
            IStatBuilder xStat,
            params (double x, double y)[] points)
        {
            if (points.Length < 2)
                throw new ArgumentException("At least two points necessary", nameof(points));

            // Each section (between two points) describes one linear function
            var sections = new List<(IConditionBuilder condition, ValueBuilder multiplier)>();
            var last = points[0];
            foreach (var (x2, y2) in points.Skip(1))
            {
                var (x1, y1) = last;
                if (x2 <= x1)
                    throw new ArgumentException("Each x must be greater than the previous", nameof(points));

                // Linear function: y = m * x + b
                // Calculate m from two points
                var m = (y2 - y1) / (x2 - x1);
                // Calculate b from m and one point (b = y - m * x)
                var b = y2 - m * x2;
                sections.Add((xStat.Value <= x2, m * xStat.Value + b));
                last = (x2, y2);
            }

            // Constant multiplier before first and after last section
            var firstCondition = xStat.Value <= points[0].x;
            var firstMultiplier = points[0].y;
            var lastMultiplier = last.y;

            var builder = valueFactory.If(firstCondition).Then(firstMultiplier);
            foreach (var (condition, multiplier) in sections)
            {
                builder = builder.ElseIf(condition).Then(multiplier);
            }

            return builder.Else(lastMultiplier);
        }
    }
}