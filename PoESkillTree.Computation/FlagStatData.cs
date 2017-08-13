using System.Collections.Generic;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation
{
    public class FlagStatData
    {
        public FlagStatData(IFlagStatProvider flag, IReadOnlyList<string> statLines)
        {
            Flag = flag;
            StatLines = statLines;
        }

        public IFlagStatProvider Flag { get; }

        public IReadOnlyList<string> StatLines { get; }
    }
}