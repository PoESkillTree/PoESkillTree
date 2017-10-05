using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class PoolStatMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public PoolStatMatchers(IBuilderFactories builderFactories, 
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => false;

        public IEnumerable<MatcherData> Matchers => 
            new StatMatcherCollection<IPoolStatBuilder>(_modifierBuilder, ValueFactory)
            {
                { "(maximum )?life", Life },
                { "(maximum )?mana", Mana },
                { "(maximum )?energy shield", EnergyShield },
            };
    }
}