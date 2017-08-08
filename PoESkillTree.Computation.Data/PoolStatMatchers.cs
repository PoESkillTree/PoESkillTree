using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data
{
    public class PoolStatMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IMatchBuilder _matchBuilder;

        public PoolStatMatchers(IProviderFactories providerFactories, 
            IMatchContextFactory matchContextFactory, IMatchBuilder matchBuilder) 
            : base(providerFactories, matchContextFactory)
        {
            _matchBuilder = matchBuilder;
            Matchers = CreateCollection().ToList();
        }

        public IReadOnlyList<MatcherData> Matchers { get; }

        private StatMatcherCollection<IPoolStatProvider> CreateCollection() =>
            new StatMatcherCollection<IPoolStatProvider>(_matchBuilder)
            {
                { "(maximum )?life", Life },
                { "(maximum )?mana", Mana },
                { "(maximum )?energy shield", EnergyShield },
            };
    }
}