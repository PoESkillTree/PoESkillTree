using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data
{
    public class FlagMatchers : IReferencedMatchers<IFlagStatProvider>
    {
        private IFlagStatProviderFactory Flag { get; }

        public FlagMatchers(IFlagStatProviderFactory flagStatProviderFactory)
        {
            Flag = flagStatProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        public string ReferenceName { get; } = nameof(FlagMatchers);

        public IReadOnlyList<ReferencedMatcherData<IFlagStatProvider>> Matchers { get; }

        private ReferencedMatcherCollection<IFlagStatProvider> CreateCollection() =>
            new ReferencedMatcherCollection<IFlagStatProvider>
            {
                { "onslaught", Flag.Onslaught },
                { "unholy might", Flag.UnholyMight },
                { "phasing", Flag.Phasing },
            };
    }
}