using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <summary>
    /// Entries of <see cref="IIntermediateModifier"/>. Implemented as an immutable class that allows changes
    /// through a fluent interface creating new instances on each method call. Since <see cref="IIntermediateModifier"/>
    /// is for partial modifiers, every property can be null.
    /// </summary>
    public class IntermediateModifierEntry : ValueObject
    {
        [CanBeNull]
        public IFormBuilder Form { get; }

        [CanBeNull]
        public IStatBuilder Stat { get; }

        [CanBeNull]
        public IValueBuilder Value { get; }

        [CanBeNull]
        public IConditionBuilder Condition { get; }

        public IntermediateModifierEntry()
        {
        }

        private IntermediateModifierEntry(IFormBuilder form, IStatBuilder stat, IValueBuilder value, 
            IConditionBuilder condition)
        {
            Form = form;
            Stat = stat;
            Value = value;
            Condition = condition;
        }

        public IntermediateModifierEntry WithForm(IFormBuilder form)
        {
            return new IntermediateModifierEntry(form, Stat, Value, Condition);
        }

        public IntermediateModifierEntry WithStat(IStatBuilder stat)
        {
            return new IntermediateModifierEntry(Form, stat, Value, Condition);
        }

        public IntermediateModifierEntry WithValue(IValueBuilder value)
        {
            return new IntermediateModifierEntry(Form, Stat, value, Condition);
        }

        public IntermediateModifierEntry WithCondition(IConditionBuilder condition)
        {
            return new IntermediateModifierEntry(Form, Stat, Value, condition);
        }

        protected override object ToTuple() => (Form, Stat, Value, Condition);
    }
}