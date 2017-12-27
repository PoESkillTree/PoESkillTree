using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    /// <summary>
    /// Entries of <see cref="IIntermediateModifier"/>. Implemented as an immutable class that allows changes
    /// through a fluent interface creating new instances on each method call. Since <see cref="IIntermediateModifier"/>
    /// is for partial modifiers, every property can be null.
    /// </summary>
    public class IntermediateModififerEntry
    {
        [CanBeNull]
        public IFormBuilder Form { get; }

        [CanBeNull]
        public IStatBuilder Stat { get; }

        [CanBeNull]
        public IValueBuilder Value { get; }

        [CanBeNull]
        public IConditionBuilder Condition { get; }

        public IntermediateModififerEntry()
        {
        }

        private IntermediateModififerEntry(IFormBuilder form, IStatBuilder stat, IValueBuilder value, 
            IConditionBuilder condition)
        {
            Form = form;
            Stat = stat;
            Value = value;
            Condition = condition;
        }

        public IntermediateModififerEntry WithForm(IFormBuilder form)
        {
            return new IntermediateModififerEntry(form, Stat, Value, Condition);
        }

        public IntermediateModififerEntry WithStat(IStatBuilder stat)
        {
            return new IntermediateModififerEntry(Form, stat, Value, Condition);
        }

        public IntermediateModififerEntry WithValue(IValueBuilder value)
        {
            return new IntermediateModififerEntry(Form, Stat, value, Condition);
        }

        public IntermediateModififerEntry WithCondition(IConditionBuilder condition)
        {
            return new IntermediateModififerEntry(Form, Stat, Value, condition);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is IntermediateModififerEntry other))
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
            return $"IntermediateModififerEntry(Form={Form},Stat={Stat},Value={Value},Condition={Condition})";
        }
    }
}