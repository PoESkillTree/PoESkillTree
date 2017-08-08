using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;

namespace PoESkillTree.Computation.Data
{
    public class PropertyMatchers : UsesMatchContext, IStatMatchers
    {
        // Used to match properties of items and skills
        // "Elemental Damage: ..." needs to be replaced by up to three properties (one for each 
        // element) before it gets here.

        private readonly IMatchBuilder _matchBuilder;

        public PropertyMatchers(IProviderFactories providerFactories, 
            IMatchContextFactory matchContextFactory, IMatchBuilder matchBuilder)
            : base(providerFactories, matchContextFactory)
        {
            _matchBuilder = matchBuilder;
            Matchers = CreateCollection().ToList();
        }

        public IReadOnlyList<MatcherData> Matchers { get; }

        private PropertyMatcherCollection CreateCollection() => new PropertyMatcherCollection(
            _matchBuilder)
        {
            { "quality" }, // do nothing with it
            { "attacks per second", Skills.Speed },
            { "cast time", Skills.Speed, v => v.Invert },
            { "fire damage", Fire.Damage },
            { "damage effectiveness", Skills.DamageEffectiveness }
        };
    }
}