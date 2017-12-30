using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="PoESkillTree.Computation.Parsing.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of a condition, that allows collection 
    /// initialization syntax for adding entries.
    /// </summary>
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