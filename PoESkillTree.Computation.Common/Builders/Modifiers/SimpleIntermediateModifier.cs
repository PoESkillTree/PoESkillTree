using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

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
        public Func<IStatBuilder, IStatBuilder> StatConverter { get; }
        public Func<IValueBuilder, IValueBuilder> ValueConverter { get; }

        public SimpleIntermediateModifier(IReadOnlyList<IntermediateModifierEntry> entries,
            Func<IStatBuilder, IStatBuilder> statConverter,
            Func<IValueBuilder, IValueBuilder> valueConverter)
        {
            Entries = entries;
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }
    }
}