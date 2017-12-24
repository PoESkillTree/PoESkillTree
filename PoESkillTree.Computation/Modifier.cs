using JetBrains.Annotations;

namespace PoESkillTree.Computation
{
    public class Modifier
    {
        // An IParser<IReadOnlyList<Modifier>> instance will be the interface between Computation
        // and Computation.Parsing.
        // The actual types for the properties are yet to be determined.

        [CanBeNull]
        public object Stat { get; }

        [CanBeNull]
        public object Form { get; }

        [CanBeNull]
        public object Value { get; }

        [CanBeNull]
        public object Condition { get; }

        public Modifier(object stat, object form, object value, object condition)
        {
            Stat = stat;
            Form = form;
            Value = value;
            Condition = condition;
        }

        private bool Equals(Modifier other)
        {
            return Equals(Stat, other.Stat)
                   && Equals(Form, other.Form)
                   && Equals(Value, other.Value)
                   && Equals(Condition, other.Condition);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Modifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Stat != null ? Stat.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Form != null ? Form.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Condition != null ? Condition.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Stat: {Stat}\n  Form: {Form}\n  Value: {Value}\n  Condition: {Condition}";
        }
    }
}