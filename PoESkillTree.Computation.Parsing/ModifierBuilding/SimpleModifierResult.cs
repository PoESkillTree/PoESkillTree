using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public class SimpleModifierResult : IModifierResult
    {
        public static readonly IModifierResult Empty =
            new SimpleModifierResult(new ModifierResultEntry[0], s => s, v => v);

        public IReadOnlyList<ModifierResultEntry> Entries { get; }
        public Func<IStatBuilder, IStatBuilder> StatConverter { get; }
        public Func<IValueBuilder, IValueBuilder> ValueConverter { get; }

        public SimpleModifierResult(IReadOnlyList<ModifierResultEntry> entries,
            Func<IStatBuilder, IStatBuilder> statConverter,
            Func<IValueBuilder, IValueBuilder> valueConverter)
        {
            Entries = entries;
            StatConverter = statConverter;
            ValueConverter = valueConverter;
        }
    }
}