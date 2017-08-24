using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Data
{
    public class FlagStatData
    {
        public FlagStatData(IFlagStatBuilder flag, IReadOnlyList<string> statLines)
        {
            Flag = flag;
            StatLines = statLines;
        }

        public IFlagStatBuilder Flag { get; }

        public IReadOnlyList<string> StatLines { get; }
    }
}