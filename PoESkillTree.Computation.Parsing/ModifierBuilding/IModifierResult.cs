using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierResult
    {
        IReadOnlyList<ModifierBuilderEntry> Entries { get; }

        Func<IStatBuilder, IStatBuilder> StatConverter { get; }

        ValueFunc ValueConverter { get; }
    }
}