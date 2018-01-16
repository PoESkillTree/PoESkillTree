using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <inheritdoc />
    /// <summary>
    /// Immutable implementation of <see cref="IIntermediateModifier" /> that gets all properties passed via
    /// constructor.
    /// </summary>
    public class SimpleIntermediateModifier : IIntermediateModifier
    {
        /// <summary>
        /// <see cref="IIntermediateModifier"/> with no entries and identity converters.
        /// </summary>
        public static readonly IIntermediateModifier Empty =
            new SimpleIntermediateModifier(new IntermediateModifierEntry[0], s => s, v => v);

        public IReadOnlyList<IntermediateModifierEntry> Entries { get; }
        public StatConverter StatConverter { get; }
        public ValueConverter ValueConverter { get; }

        public SimpleIntermediateModifier(IReadOnlyList<IntermediateModifierEntry> entries,
            StatConverter statConverter,
            ValueConverter valueConverter)
        {
            Entries = entries;
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }
    }
}