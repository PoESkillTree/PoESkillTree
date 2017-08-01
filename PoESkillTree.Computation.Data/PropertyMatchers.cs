using System.Collections.Generic;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;

namespace PoESkillTree.Computation.Data
{
    public class PropertyMatchers : UsesMatchContext, IStatMatchers
    {
        // Used to match properties of items and skills

        private IMatchConditionFactory MatchCondition { get; }

        public PropertyMatchers(IProviderFactories providerFactories, 
            IMatchContextFactory matchContextFactory, IMatchConditionFactory matchConditionFactory)
            : base(providerFactories, matchContextFactory)
        {
            MatchCondition = matchConditionFactory;

            StatMatchers = CreateCollection();
        }

        public IEnumerable<object> StatMatchers { get; }

        private PropertyMatcherCollection CreateCollection() => new PropertyMatcherCollection
        {
            { "quality" }, // do nothing with it
            { "attacks per second", Skills.Speed },
            { "cast time", Skills.Speed, v => v.Invert },
            { "elemental damage", Fire.Damage, MatchCondition.MatchHas(ValueColoring.Fire) },
            { "damage effectiveness", Skills.DamageEffectiveness }
        };
    }
}