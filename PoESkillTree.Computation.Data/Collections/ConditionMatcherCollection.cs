using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ConditionMatcherCollection : MatcherCollection
    {
        public ConditionMatcherCollection(IMatchBuilder matchBuilder) : base(matchBuilder)
        {
        }

        public void Add([RegexPattern] string regex, IConditionProvider condition)
        {
            Add(regex, MatchBuilder.WithCondition(condition));
        }
    }
}