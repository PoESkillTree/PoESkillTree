using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data
{
    public class FormMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<MatcherData>> _lazyMatchers;

        public FormMatchers(IBuilderFactories builderFactories,
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
            _lazyMatchers = new Lazy<IReadOnlyList<MatcherData>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<MatcherData> Matchers => _lazyMatchers.Value;

        private FormMatcherCollection CreateCollection() => new FormMatcherCollection(_modifierBuilder,
            ValueFactory)
        {
            { "#% increased", PercentIncrease, Value },
            { "#% reduced", PercentReduce, Value },
            { "#% more", PercentMore, Value },
            { "#% less", PercentLess, Value },
            { @"\+#%? to", BaseAdd, Value },
            { @"\+?#%?(?= chance)", BaseAdd, Value },
            { @"\+?#% of", BaseAdd, Value },
            { "gain #% of", BaseAdd, Value },
            { "gain #", BaseAdd, Value },
            { "#% additional", BaseAdd, Value },
            { "an additional", BaseAdd, 1 },
            { @"-#% of", BaseSubtract, Value },
            { "-#%? to", BaseSubtract, Value },
            { "can (have|summon) up to # additional", MaximumAdd, Value },
        };
    }
}