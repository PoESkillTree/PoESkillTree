using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Data
{
    /// <summary>
    /// Collection of the stats that are applied when effects are active and flag stats are on.
    /// </summary>
    public interface IEffectStats
    {
        IReadOnlyList<EffectStatData> Effects { get; }

        IReadOnlyList<FlagStatData> Flags { get; }
    }
}