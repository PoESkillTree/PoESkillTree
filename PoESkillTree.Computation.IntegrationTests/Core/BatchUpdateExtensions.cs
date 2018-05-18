using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal static class BatchUpdateExtensions
    {
        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, IValue value) =>
            batch.AddModifier(new[] { stat }, form, value, new GlobalModifierSource());

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IStat stat, Form form, IValue value, IModifierSource source) =>
            batch.AddModifier(new[] { stat }, form, value, source);

        public static CalculatorExtensions.BatchUpdate AddModifier(this CalculatorExtensions.BatchUpdate batch,
            IReadOnlyList<IStat> stats, Form form, IValue value, IModifierSource source) =>
            batch.AddModifier(new Modifier(stats, form, value, source));
    }
}