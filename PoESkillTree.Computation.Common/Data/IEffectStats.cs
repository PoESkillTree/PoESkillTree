using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Collection of the stats that are applied when effects are active.
    /// </summary>
    public interface IEffectStats
    {
        IReadOnlyList<EffectStatData> Effects { get; }
    }
}