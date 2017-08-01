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

        public IReadOnlyList<(string regex, IFlagStatProvider match)> Matchers { get; }

        private MatcherCollection<IFlagStatProvider> CreateCollection() =>
            new MatcherCollection<IFlagStatProvider>
            {
                { "onslaught", Flag.Onslaught },
                { "unholy might", Flag.UnholyMight },
                { "phasing", Flag.Phasing },
            };
    }
}