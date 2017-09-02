using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public class EmptyModifierResult : IModifierResult
    {
        public IReadOnlyList<ModifierBuilderEntry> Entries { get; } = new ModifierBuilderEntry[0];
        public Func<IStatBuilder, IStatBuilder> StatConverter { get; } = s => s;
        public Func<IValueBuilder, IValueBuilder> ValueConverter { get; } = v => v;
    }
}