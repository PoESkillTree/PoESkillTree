using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public class ModifierResultEntry
    {
        [CanBeNull]
        public IFormBuilder Form { get; }

        [CanBeNull]
        public IStatBuilder Stat { get; }

        [CanBeNull]
        public IValueBuilder Value { get; }

        [CanBeNull]
        public IConditionBuilder Condition { get; }

        public ModifierResultEntry()
        {
        }

        private ModifierResultEntry(IFormBuilder form, IStatBuilder stat, IValueBuilder value, 
            IConditionBuilder condition)
        {
            Form = form;
            Stat = stat;
            Value = value;
            Condition = condition;
        }

        public ModifierResultEntry WithForm(IFormBuilder form)
        {
            return new ModifierResultEntry(form, Stat, Value, Condition);
        }

        public ModifierResultEntry WithStat(IStatBuilder stat)
        {
            return new ModifierResultEntry(Form, stat, Value, Condition);
        }

        public ModifierResultEntry WithValue(IValueBuilder value)
        {
            return new ModifierResultEntry(Form, Stat, value, Condition);
        }

        public ModifierResultEntry WithCondition(IConditionBuilder condition)
        {
            return new ModifierResultEntry(Form, Stat, Value, condition);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is ModifierResultEntry other))
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
            return $"ModifierResultEntry(Form={Form},Stat={Stat},Value={Value},Condition={Condition})";
        }
    }
}