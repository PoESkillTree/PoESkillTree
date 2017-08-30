using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class FormMatchers : UsesFormProviders, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<MatcherData>> _lazyMatchers;

        public FormMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder) 
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyMatchers = new Lazy<IReadOnlyList<MatcherData>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<MatcherData> Matchers => _lazyMatchers.Value;

        private FormMatcherCollection CreateCollection() => new FormMatcherCollection(_modifierBuilder,
            ValueFactory)
        {
            { "#% increased", PercentIncrease },
            { "#% reduced", PercentReduce },
            { "#% more", PercentMore },
            { "#% less", PercentLess },
            { @"\+#%? to", BaseAdd },
            { @"\+?#%?(?= chance)", BaseAdd },
            { @"\+?#% of", BaseAdd },
            { "gain #% of", BaseAdd },
            { "gain #", BaseAdd },
            { "#% additional", BaseAdd },
            { "an additional", BaseAdd, 1 },
            { @"-#% of", BaseSubtract },
            { "-#%? to", BaseSubtract },
            { "can (have|summon) up to # additional", MaximumAdd },
        };
    }
}