using JetBrains.Annotations;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Modifiers
{
    /// <summary>
    /// Entries of <see cref="IIntermediateModifier"/>. Implemented as an immutable class that allows changes
    /// through a fluent interface creating new instances on each method call. Since <see cref="IIntermediateModifier"/>
    /// is for partial modifiers, every property can be null.
    /// </summary>
    public class IntermediateModifierEntry
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

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is IntermediateModifierEntry other))
                return false;

            return Equals(Form, other.Form)
                   && Equals(Stat, other.Stat)
                   && Equals(Value, other.Value)
                   && Equals(Condition, other.Condition);
        }

        public override int GetHashCode()
        {
            return (Form != null ? Form.GetHashCode() : 0) ^
                   (Stat != null ? Stat.GetHashCode() : 0) ^
                   (Value != null ? Value.GetHashCode() : 0) ^
                   (Condition != null ? Condition.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return $"IntermediateModifierEntry(Form={Form},Stat={Stat},Value={Value},Condition={Condition})";
        }
    }
}