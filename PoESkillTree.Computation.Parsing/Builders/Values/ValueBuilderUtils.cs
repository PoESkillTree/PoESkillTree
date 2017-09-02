using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Values
{
    public static class ValueBuilderUtils
    {
        public static Func<ValueBuilder, ValueBuilder> PerStat(IStatBuilder stat) => 
            v => v * (stat.Value / 1).Floored;

        public static Func<ValueBuilder, ValueBuilder> PerStat(IStatBuilder stat, ValueBuilder divideBy) => 
            v => v * (stat.Value / divideBy).Floored;

        public static Func<ValueBuilder, ValueBuilder> PerStatCeiled(IStatBuilder stat, ValueBuilder divideBy) =>
            v => v * (stat.Value / divideBy).Ceiled;

        public static Func<ValueBuilder, ValueBuilder> PercentOf(IStatBuilder stat) =>
            v => stat.Value * v.AsPercentage;

        public static ValueBuilder LinearScale(this IValueBuilders valueFactory, 
            IStatBuilder yStat,
            params (double y, double multiplier)[] points)
        {
            if (points.Length < 2)
                throw new ArgumentException("At least two points necessary", nameof(points));

            // Each section (between two points) describes one linear function
            var sections = new List<(IConditionBuilder condition, ValueBuilder multiplier)>();
            (double x, double y) last = points[0];
            foreach (var (x2, y2) in points.Skip(1))
            {
                var (x1, y1) = last;
                if (x2 <= x1)
                    throw new ArgumentException("Each y must be greater than the previous", 
                        nameof(points));

                // Linear function: y = m * x + b
                // Calculate m from two points
                var m = (y2 - y1) / (x2 - x1);
                // Calculate b from m and one point (b = y - m * x)
                var b = y2 - m * x2;
                sections.Add((yStat.Value <= x2, m * yStat.Value + b));
                last = (x2, y2);
            }
            // Constant multiplier before first and after last section
            var firstCondition = yStat.Value <= points[0].y;
            var firstMultiplier = points[0].multiplier;
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