using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    public class FlagMatchers : ReferencedMatchersBase<IFlagStatBuilder>
    {
        private IFlagStatBuilders Flag { get; }

        public FlagMatchers(IFlagStatBuilders flagStatBuilders)
        {
            Flag = flagStatBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<IFlagStatBuilder>
            {
                { "onslaught", Flag.Onslaught },
                { "unholy might", Flag.UnholyMight },
                { "phasing", Flag.Phasing },
            };
    }
}