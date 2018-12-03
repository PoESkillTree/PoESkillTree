using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents the result of <see cref="IStatBuilder"/>. Each of these will be built into one <see cref="Modifier"/>
    /// </summary>
    public class StatBuilderResult : ValueObject
    {
        public StatBuilderResult(
            IReadOnlyList<IStat> stats, ModifierSource modifierSource, ValueConverter valueConverter)
        {
            Stats = stats;
            ModifierSource = modifierSource;
            ValueConverter = valueConverter;
        }

        public IReadOnlyList<IStat> Stats { get; }

        public ModifierSource ModifierSource { get; }

        /// <summary>
        /// <see cref="ValueConverter"/> to apply to value builders before building them.
        /// </summary>
        public ValueConverter ValueConverter { get; }

        public void Deconstruct(
            out IReadOnlyList<IStat> stats, out ModifierSource modifierSource, out ValueConverter valueConverter) =>
            (stats, modifierSource, valueConverter) = (Stats, ModifierSource, ValueConverter);

        public static implicit operator StatBuilderResult(
            (IReadOnlyList<IStat> stats, ModifierSource modifierSource, ValueConverter valueConverter) tuple) =>
            new StatBuilderResult(tuple.stats, tuple.modifierSource, tuple.valueConverter);

        protected override object ToTuple() => (WithSequenceEquality(Stats), ModifierSource, ValueConverter);
    }
}