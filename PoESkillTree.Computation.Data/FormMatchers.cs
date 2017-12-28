using System.Collections;
using System.Collections.Generic;
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

        public FormMatchers(IBuilderFactories builderFactories,
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder) 
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => false;

        public IEnumerator<MatcherData> GetEnumerator() => new FormMatcherCollection(_modifierBuilder,
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
            { "(?<!chance to )gain #", BaseAdd, Value },
            { "#% additional", BaseAdd, Value },
            { "an additional", BaseAdd, 1 },
            { "adds # to", BaseAdd, Value },
            { @"-#% of", BaseSubtract, Value },
            { "-#%? to", BaseSubtract, Value },
            { "can (have|summon) up to # additional", BaseAdd, Value },
        }.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}