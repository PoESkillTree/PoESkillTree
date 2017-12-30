using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="PoESkillTree.Computation.Parsing.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of a value converter, that allows collection 
    /// initialization syntax for adding entries.
    /// </summary>
    public class ValueConversionMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public ValueConversionMatcherCollection(IModifierBuilder modifierBuilder,
            IValueBuilders valueFactory) : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, Func<ValueBuilder, ValueBuilder> func)
        {
            Add(regex, ModifierBuilder.WithValueConverter(_valueFactory.WrapValueConverter(func)));
        }

        public void Add([RegexPattern] string regex, ValueBuilder multiplier)
        {
            Add(regex, v => v * multiplier);
        }
    }
}