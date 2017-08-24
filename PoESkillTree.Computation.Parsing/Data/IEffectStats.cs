using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    public interface IEffectStats
    {
        IReadOnlyList<EffectStatData> Effects { get; }

        IReadOnlyList<FlagStatData> Flags { get; }
    }
}