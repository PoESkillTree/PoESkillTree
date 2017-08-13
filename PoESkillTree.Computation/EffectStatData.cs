using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation
{
    public class EffectStatData
    {
        public EffectStatData(IEffectProvider effect, IReadOnlyList<string> statLines)
        {
            Effect = effect;
            StatLines = statLines;
            FlagStats = new IFlagStatProvider[0];
        }

        public EffectStatData(IEffectProvider effect, IReadOnlyList<IFlagStatProvider> flagStats)
        {
            Effect = effect;
            StatLines = new string[0];
            FlagStats = flagStats;
        }

        public IEffectProvider Effect { get; }

        public IReadOnlyList<string> StatLines { get; }

        public IReadOnlyList<IFlagStatProvider> FlagStats { get; }
    }
}