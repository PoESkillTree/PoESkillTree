using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Data that specifies the stat lines that should be applied if a flag stat's value is set to on.
    /// </summary>
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