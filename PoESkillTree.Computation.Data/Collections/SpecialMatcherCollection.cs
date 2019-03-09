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
    /// <see cref="IIntermediateModifier"/>s consisting only of one more tuples of a form, value, stat and
    /// optionally a condition,
    /// that allows collection initialization syntax for adding entries.
    /// </summary>
    /// <remarks>
    /// The difference between this and <see cref="FormAndStatMatcherCollection"/> is that this collection is
    /// used for matchers that match the whole stat, with no other matchers applied afterwards.
    /// </remarks>
    public class SpecialMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public SpecialMatcherCollection(IModifierBuilder modifierBuilder, IValueBuilders valueFactory)
            : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        /// <summary>
        /// Adds a matcher with a form, value, stat and optionally a condition.
        /// </summary>
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
        /// Adds a matcher with a form, value, stat and optionally a condition.
        /// </summary>
        public void Add([RegexPattern] string regex, IFormBuilder form, double value, IStatBuilder stat,
            IConditionBuilder condition = null)
        {
            Add(regex, form, _valueFactory.Create(value), stat, condition);
        }

        /// <summary>
        /// Adds a matcher with a form, value and stats.
        /// </summary>
        public void Add([RegexPattern] string regex, IFormBuilder form, double value, params IStatBuilder[] stats)
            => Add(regex, form, _valueFactory.Create(value), stats);

        /// <summary>
        /// Adds a matcher with a form, value and stats.
        /// </summary>
        public void Add(
            [RegexPattern] string regex, IFormBuilder form, IValueBuilder value, params IStatBuilder[] stats)
        {
            var builder = ModifierBuilder
                .WithForm(form)
                .WithStats(stats)
                .WithValue(value);
            Add(regex, builder);
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