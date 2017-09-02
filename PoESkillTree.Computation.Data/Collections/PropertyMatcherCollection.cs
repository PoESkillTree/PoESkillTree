using System;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class PropertyMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public PropertyMatcherCollection(IModifierBuilder modifierBuilder,
            IValueBuilders valueFactory) : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex)
        {
            Add(regex, ModifierBuilder);
        }

        public void Add([RegexPattern] string regex, IStatBuilder stat)
        {
            Add(regex, ModifierBuilder.WithStat(stat));
        }

        public void Add([RegexPattern] string regex, IStatBuilder stat, 
            Func<ValueBuilder, ValueBuilder> converter)
        {
            var builder = ModifierBuilder
                .WithStat(stat)
                .WithValueConverter(_valueFactory.WrapValueConverter(converter));
            Add(regex, builder);
        }
    }
}