using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public class SimpleModifierResult : IModifierResult
    {
        public static readonly IModifierResult Empty =
            new SimpleModifierResult(new ModifierBuilderEntry[0], s => s, v => v);

        public IReadOnlyList<ModifierBuilderEntry> Entries { get; }
        public Func<IStatBuilder, IStatBuilder> StatConverter { get; }
        public Func<IValueBuilder, IValueBuilder> ValueConverter { get; }

        public SimpleModifierResult(IReadOnlyList<ModifierBuilderEntry> entries,
            Func<IStatBuilder, IStatBuilder> statConverter,
            Func<IValueBuilder, IValueBuilder> valueConverter)
        {
            Entries = entries;
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }
    }
}