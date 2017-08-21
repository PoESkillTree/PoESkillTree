using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data
{
    public class FlagMatchers : ReferencedMatchersBase<IFlagStatProvider>
    {
        private IFlagStatProviderFactory Flag { get; }

        public FlagMatchers(IFlagStatProviderFactory flagStatProviderFactory)
        {
            Flag = flagStatProviderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData<IFlagStatProvider>>
            CreateCollection() =>
            new ReferencedMatcherCollection<IFlagStatProvider>
            {
                { "onslaught", Flag.Onslaught },
                { "unholy might", Flag.UnholyMight },
                { "phasing", Flag.Phasing },
            };
    }
}