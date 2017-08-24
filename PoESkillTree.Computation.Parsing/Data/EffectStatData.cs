using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Data
{
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