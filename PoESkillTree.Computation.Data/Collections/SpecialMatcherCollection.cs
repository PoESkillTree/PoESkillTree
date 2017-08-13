using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Forms;
using PoESkillTree.Computation.Providers.Stats;
using PoESkillTree.Computation.Providers.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    public class SpecialMatcherCollection : MatcherCollection
    {
        private readonly IValueProviderFactory _valueFactory;

        public SpecialMatcherCollection(IMatchBuilder matchBuilder, 
            IValueProviderFactory valueFactory) : base(matchBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            ValueProvider value, IConditionProvider condition = null)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValue(value);
            if (condition != null)
            {
                builder = builder.WithCondition(condition);
            }
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            double value, IConditionProvider condition = null)
        {
            Add(regex, form, stat, _valueFactory.Create(value), condition);
        }

        public void Add([RegexPattern] string regex, IFormProvider form, IStatProvider stat,
            IConditionProvider condition = null)
        {
            var builder = MatchBuilder
                .WithForm(form)
                .WithStat(stat);
            if (condition != null)
            {
                builder = builder.WithCondition(condition);
            }
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex,
            params (IFormProvider form, IStatProvider stat, ValueProvider value,
                IConditionProvider condition)[] stats)
        {
            var formList = new List<IFormProvider>();
            var statList = new List<IStatProvider>();
            var valueList = new List<ValueProvider>();
            var conditionList = new List<IConditionProvider>();
            foreach (var (form, stat, value, condition) in stats)
            {
                formList.Add(form);
                statList.Add(stat);
                valueList.Add(value);
                conditionList.Add(condition);
            }

            var builder = MatchBuilder
                .WithForms(formList)
                .WithStats(statList)
                .WithValues(valueList)
                .WithConditions(conditionList);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex,
            params (IFormProvider form, IStatProvider stat, double value,
                IConditionProvider condition)[] stats)
        {
            var withValueProviders =
                stats.Select(t => (t.form, t.stat, _valueFactory.Create(t.value), t.condition));
            Add(regex, withValueProviders.ToArray());
        }
    }
}