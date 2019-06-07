using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of <see cref="Common.Data.MatcherData"/>, with 
    /// <see cref="IIntermediateModifier"/>s consisting only of forms, values, stats and sometimes conditions,
    /// that allows collection initialization syntax for adding entries.
    /// </summary>
    public class FormAndStatMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public FormAndStatMatcherCollection(IModifierBuilder modifierBuilder, IValueBuilders valueFactory)
            : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        /// <summary>
        /// Adds a matcher with a form, value, stat and optionally a condition.
        /// </summary>
        public void Add([RegexPattern] string regex, IFormBuilder form, double value, IStatBuilder stat,
            IConditionBuilder condition = null)
        {
            Add(regex, form, _valueFactory.Create(value), stat, condition);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, IStatBuilder stat,
            IConditionBuilder condition = null)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStat(stat)
                .WithValue(value);
            if (condition != null)
            {
                builder = builder.WithCondition(condition);
            }

            Add(regex, builder);
        }

        /// <summary>
        /// Adds a matcher with a form, value and one or more stats.
        /// </summary>
        public void Add(
            [RegexPattern] string regex, IFormBuilder form, IValueBuilder value, params IStatBuilder[] stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value)
                .WithStats(stats);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, double value, params IStatBuilder[] stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(_valueFactory.Create(value))
                .WithStats(stats);
            Add(regex, builder);
        }

        /// <summary>
        /// Adds a matcher with a form, value and one or more stats.
        /// </summary>
        public void Add([RegexPattern] string regex, IFormBuilder form, double value, IEnumerable<IStatBuilder> stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(_valueFactory.Create(value))
                .WithStats(stats.ToList());
            Add(regex, builder);
        }

        /// <summary>
        /// Adds a substituting matcher with a form, value and stat.
        /// </summary>
        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, IStatBuilder stat,
            string substitution)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithValue(value)
                .WithStat(stat);
            Add(regex, builder, substitution);
        }

        public void Add(
            [RegexPattern] string regex, params (IFormBuilder form, double value, IStatBuilder stat)[] stats)
        {
            var withIValueBuilders = stats.Select(t => (t.form, _valueFactory.Create(t.value), t.stat));
            Add(regex, withIValueBuilders.ToArray());
        }

        public void Add(
            [RegexPattern] string regex, params (IFormBuilder form, IValueBuilder value, IStatBuilder stat)[] stats)
        {
            var formList = new List<IFormBuilder>();
            var valueList = new List<IValueBuilder>();
            var statList = new List<IStatBuilder>();
            foreach (var (form, value, stat) in stats)
            {
                formList.Add(form);
                valueList.Add(value);
                statList.Add(stat);
            }

            var builder = ModifierBuilder
                .WithForms(formList)
                .WithValues(valueList)
                .WithStats(statList);
            Add(regex, builder);
        }

        /// <summary>
        /// Adds a matcher with multiple (form, value, stat, condition) tuples.
        /// </summary>
        public void Add(
            [RegexPattern] string regex,
            params (IFormBuilder form, IValueBuilder value, IStatBuilder stat, IConditionBuilder condition)[] stats)
        {
            var formList = new List<IFormBuilder>();
            var valueList = new List<IValueBuilder>();
            var statList = new List<IStatBuilder>();
            var conditionList = new List<IConditionBuilder>();
            foreach (var (form, value, stat, condition) in stats)
            {
                formList.Add(form);
                valueList.Add(value);
                statList.Add(stat);
                conditionList.Add(condition);
            }

            var builder = ModifierBuilder
                .WithForms(formList)
                .WithValues(valueList)
                .WithStats(statList)
                .WithConditions(conditionList);
            Add(regex, builder);
        }

        /// <summary>
        /// Adds a matcher with multiple (form, value, stat, condition) tuples.
        /// </summary>
        public void Add(
            [RegexPattern] string regex,
            params (IFormBuilder form, double value, IStatBuilder stat, IConditionBuilder condition)[] stats)
        {
            var withIValueBuilders = stats.Select(t => (t.form, _valueFactory.Create(t.value), t.stat, t.condition));
            Add(regex, withIValueBuilders.ToArray());
        }
    }
}