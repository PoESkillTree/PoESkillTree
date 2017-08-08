using System.Collections.Generic;
using JetBrains.Annotations;
using MoreLinq;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class FormAndStatMatcherCollection : MatcherCollection
    {
        private readonly IValueProviderFactory _valueFactory;

        public FormAndStatMatcherCollection(IMatchBuilder matchBuilder,
            IValueProviderFactory valueFactory)
            : base(matchBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, 
            double value, IConditionProvider condition = null)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValue(_valueFactory.Create(value));
            if (condition != null)
            {
                builder = builder.WithCondition(condition);
            }
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, 
            params IStatProvider[] stats)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStats(stat.Concat(stats));
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IEnumerable<IStatProvider> stats)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, string substitution)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat, ValueFunc converter)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValueConverter(converter);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, 
            (IFormProvider forFirstValue, IFormProvider forSecondValue) forms, IStatProvider stat)
        {
            var builder = MatchBuilder
                .WithForms(new[] { forms.forFirstValue, forms.forSecondValue })
                .WithStat(stat);
            Add(regex, builder);
        }
    }
}