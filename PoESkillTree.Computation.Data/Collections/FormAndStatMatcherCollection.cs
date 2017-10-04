using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MoreLinq;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FormAndStatMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public FormAndStatMatcherCollection(IModifierBuilder modifierBuilder,
            IValueBuilders valueFactory)
            : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, double value, 
            IStatBuilder stat, IConditionBuilder condition = null)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValue(_valueFactory.Create(value));
            if (condition != null)
            {
                builder = builder.WithCondition(condition);
            }
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, 
            IStatBuilder stat, params IStatBuilder[] stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value)
                .WithStats(stat.Concat(stats));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, double value, 
            IEnumerable<IStatBuilder> stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(_valueFactory.Create(value))
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, 
            IStatBuilder stat, string substitution)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value)
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, 
            IStatBuilder stat, Func<ValueBuilder, ValueBuilder> converter)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value)
                .WithStat(stat)
                .WithValueConverter(_valueFactory.WrapValueConverter(converter));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, 
            (IFormBuilder forFirstValue, IFormBuilder forSecondValue) forms,
            (IValueBuilder first, IValueBuilder second) values, IStatBuilder stat)
        {
            var builder = ModifierBuilder
                .WithForms(new[] { forms.forFirstValue, forms.forSecondValue })
                .WithValues(new[] { values.first, values.second })
                .WithStat(stat);
            Add(regex, builder);
        }
    }
}