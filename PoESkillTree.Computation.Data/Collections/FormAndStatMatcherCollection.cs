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

        public void Add([RegexPattern] string regex, IFormBuilder form, IStatBuilder stat, 
            double value, IConditionBuilder condition = null)
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

        public void Add([RegexPattern] string regex, IFormBuilder form, IStatBuilder stat, 
            params IStatBuilder[] stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStats(stat.Concat(stats));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IEnumerable<IStatBuilder> stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IStatBuilder stat, string substitution)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IStatBuilder stat, 
            Func<ValueBuilder, ValueBuilder> converter)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValueConverter(_valueFactory.WrapValueConverter(converter));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, 
            (IFormBuilder forFirstValue, IFormBuilder forSecondValue) forms, IStatBuilder stat)
        {
            var builder = ModifierBuilder
                .WithForms(new[] { forms.forFirstValue, forms.forSecondValue })
                .WithStat(stat);
            Add(regex, builder);
        }
    }
}