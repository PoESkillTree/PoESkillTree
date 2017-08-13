using System.Collections.Generic;

namespace PoESkillTree.Computation
{
    public interface IEffectStats
    {
        IReadOnlyList<EffectStatData> Effects { get; }

        IReadOnlyList<FlagStatData> Flags { get; }
    }
}