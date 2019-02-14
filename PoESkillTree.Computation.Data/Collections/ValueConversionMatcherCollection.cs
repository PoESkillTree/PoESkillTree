using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="Common.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of a value converter, that allows collection 
    /// initialization syntax for adding entries.
    /// </summary>
    public class ValueConversionMatcherCollection : MatcherCollection
    {
        public ValueConversionMatcherCollection(IModifierBuilder modifierBuilder) : base(modifierBuilder)
        {
        }

        public void Add([RegexPattern] string regex, Func<ValueBuilder, ValueBuilder> func)
        {
            Add(regex, ModifierBuilder.WithValueConverter(func.ToValueConverter()));
        }

        public void Add([RegexPattern] string regex, ValueBuilder multiplier)
        {
            Add(regex, v => v * multiplier);
        }

        public void Add(
            [RegexPattern] string regex, ValueBuilder multiplier, IConditionBuilder condition,
            string substitution = "")
        {
            var builder = ModifierBuilder
                .WithValueConverter(v => v.Multiply(multiplier))
                .WithCondition(condition);
            Add(regex, builder, substitution);
        }
    }
}