using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal static class BatchUpdateExtensions
    {
        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, double value) =>
            batch.AddModifier(stat, form, new Constant(value), new ModifierSource.Global());

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, IValue value) =>
            batch.AddModifier(stat, form, value, new ModifierSource.Global());

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, double value, ModifierSource source) =>
            batch.AddModifier(stat, form, new Constant(value), source);

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, IValue value, ModifierSource source) =>
            batch.AddModifier(new[] { stat }, form, value, source);

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, double value) =>
            batch.AddModifier(stats, form, new Constant(value));

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, IValue value) =>
            batch.AddModifier(stats, form, value, new ModifierSource.Global());

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, double value, ModifierSource source) =>
            batch.AddModifier(new Modifier(stats, form, new Constant(value), source));

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, IValue value, ModifierSource source) =>
            batch.AddModifier(new Modifier(stats, form, value, source));
    }
}