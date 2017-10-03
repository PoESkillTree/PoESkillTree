using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierResult
    {
        IReadOnlyList<ModifierResultEntry> Entries { get; }

        Func<IStatBuilder, IStatBuilder> StatConverter { get; }

        Func<IValueBuilder, IValueBuilder> ValueConverter { get; }
    }
}