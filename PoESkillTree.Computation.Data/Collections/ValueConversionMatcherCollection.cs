using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
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