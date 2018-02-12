using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IFlagStatBuilder"/>s.
    /// </summary>
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