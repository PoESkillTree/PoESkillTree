using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Values
{
    public static class ValueProviderUtils
    {
        public static ValueFunc PerStat(IStatProvider stat, ValueProvider divideBy = null) =>
            v => v * (stat.Value / divideBy ?? 1).Floored;

        public static ValueFunc PerStatCeiled(IStatProvider stat, ValueProvider divideBy) =>
            v => v * (stat.Value / divideBy ?? 1).Ceiled;

        public static ValueFunc PercentOf(IStatProvider stat) =>
            v => stat.Value * v.AsPercentage;

        public static ValueFunc LinearScale(this IFluentValueBuilder valueBuilder, 
            IStatProvider yStat,
            params (double y, double multiplier)[] points)
        {
            if (points.Length < 2)
                throw new ArgumentException("At least two points necessary", nameof(points));

            var sections = new List<(IConditionProvider condition, ValueProvider multiplier)>();
            (double x, double y) last = (double.NegativeInfinity, 1);
            foreach (var (x2, y2) in points)
            {
                var (x1, y1) = last;
                if (x2 <= x1)
                    throw new ArgumentException("Each y must be greater than the previous", 
                        nameof(points));
                // Each section (between two points) describes one linear function
                ValueProvider multiplier;
                if (double.IsNegativeInfinity(last.y))
                {
                    // Constant before first section
                    multiplier = y2;
                }
                else
                {
                    // Linear function: y = m * x + b
                    // Calculate m from two points
                    var m = (y2 - y1) / (x2 - x1);
                    // Calculate b from m and one point (b = y - m * x)
                    var b = y2 - m * x2;
                    multiplier = m * yStat.Value + b;
                }
                sections.Add((yStat.Value <= x2, multiplier));
                last = (x2, y2);
            }
            // Constant multiplier after last section
            var lastMultiplier = last.y;

            var firstSection = sections.First();
            var builder = valueBuilder.If(firstSection.condition).Then(firstSection.multiplier);
            foreach (var (condition, multiplier) in sections.Skip(1))
            {
                builder = builder.ElseIf(condition).Then(multiplier);
            }
            var conditionalMultiplier = builder.Else(lastMultiplier);

            return x => x * conditionalMultiplier;
        }

    }
}