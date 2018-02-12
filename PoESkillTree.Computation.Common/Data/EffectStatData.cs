using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Data that specifies the unparsed stat lines that should be applied and the flag stat whose values should be
    /// set to on if an effect is active.
    /// </summary>
    public class EffectStatData
    {
        public EffectStatData(IEffectBuilder effect, IReadOnlyList<string> statLines)
        {
            Effect = effect;
            StatLines = statLines;
            FlagStats = new IFlagStatBuilder[0];
        }

        public EffectStatData(IEffectBuilder effect, IReadOnlyList<IFlagStatBuilder> flagStats)
        {
            Effect = effect;
            StatLines = new string[0];
            FlagStats = flagStats;
        }

        public IEffectBuilder Effect { get; }

        public IReadOnlyList<string> StatLines { get; }

        public IReadOnlyList<IFlagStatBuilder> FlagStats { get; }
    }
}