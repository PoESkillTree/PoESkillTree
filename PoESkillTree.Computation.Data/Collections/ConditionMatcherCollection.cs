using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Data.Collections
{
    public class ConditionMatcherCollection : MatcherCollection
    {
        public ConditionMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        public void Add([RegexPattern] string regex, IConditionBuilder condition)
        {
            Add(regex, ModifierBuilder.WithCondition(condition));
        }
    }
}