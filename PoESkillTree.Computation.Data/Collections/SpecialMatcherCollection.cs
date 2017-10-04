using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;

namespace PoESkillTree.Computation.Data.Collections
{
    public class SpecialMatcherCollection : MatcherCollection
    {
        private readonly IValueBuilders _valueFactory;

        public SpecialMatcherCollection(IModifierBuilder modifierBuilder, 
            IValueBuilders valueFactory) : base(modifierBuilder)
        {
            _valueFactory = valueFactory;
        }

        public void Add([RegexPattern] string regex, IFormBuilder form, IValueBuilder value, 
            IStatBuilder stat, IConditionBuilder condition = null)
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

        public void Add([RegexPattern] string regex, IFormBuilder form, double value, 
            IStatBuilder stat, IConditionBuilder condition = null)
        {
            Add(regex, form, _valueFactory.Create(value), stat, condition);
        }

        public void Add([RegexPattern] string regex,
            params (IFormBuilder form, IStatBuilder stat, IValueBuilder value,
                IConditionBuilder condition)[] stats)
        {
            var formList = new List<IFormBuilder>();
            var statList = new List<IStatBuilder>();
            var valueList = new List<IValueBuilder>();
            var conditionList = new List<IConditionBuilder>();
            foreach (var (form, stat, value, condition) in stats)
            {
                formList.Add(form);
                statList.Add(stat);
                valueList.Add(value);
                conditionList.Add(condition);
            }

            var builder = ModifierBuilder
                .WithForms(formList)
                .WithStats(statList)
                .WithValues(valueList)
                .WithConditions(conditionList);
            Add(regex, builder);
        }

        public void Add([RegexPattern] string regex,
            params (IFormBuilder form, IStatBuilder stat, double value,
                IConditionBuilder condition)[] stats)
        {
            var withIValueBuilders =
                stats.Select(t => (t.form, t.stat, _valueFactory.Create(t.value), t.condition));
            Add(regex, withIValueBuilders.ToArray());
        }
    }
}