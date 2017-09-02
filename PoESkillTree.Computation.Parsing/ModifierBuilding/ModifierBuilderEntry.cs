using JetBrains.Annotations;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Forms;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public class ModifierBuilderEntry
    {
        [CanBeNull]
        public IFormBuilder Form { get; }

        [CanBeNull]
        public IStatBuilder Stat { get; }

        [CanBeNull]
        public IValueBuilder Value { get; }

        [CanBeNull]
        public IConditionBuilder Condition { get; }

        public ModifierBuilderEntry()
        {
        }

        private ModifierBuilderEntry(IFormBuilder form, IStatBuilder stat, IValueBuilder value, 
            IConditionBuilder condition)
        {
            Form = form;
            Stat = stat;
            Value = value;
            Condition = condition;
        }

        public ModifierBuilderEntry WithForm(IFormBuilder form)
        {
            return new ModifierBuilderEntry(form, Stat, Value, Condition);
        }

        public ModifierBuilderEntry WithStat(IStatBuilder stat)
        {
            return new ModifierBuilderEntry(Form, stat, Value, Condition);
        }

        public ModifierBuilderEntry WithValue(IValueBuilder value)
        {
            return new ModifierBuilderEntry(Form, Stat, value, Condition);
        }

        public ModifierBuilderEntry WithCondition(IConditionBuilder condition)
        {
            return new ModifierBuilderEntry(Form, Stat, Value, condition);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            if (!(obj is ModifierBuilderEntry other))
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
    }
}