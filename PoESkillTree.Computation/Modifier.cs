namespace PoESkillTree.Computation
{
    public class Modifier
    {
        // An IParser<IReadOnlyList<Modifier>> instance will be the interface between Computation
        // and Computation.Parsing.
        // The actual types for the properties are yet to be determined.

        public object Stat { get; }

        public object Form { get; }

        public object Value { get; }

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
            return Stat.Equals(other.Stat) 
                && Form.Equals(other.Form) 
                && Value.Equals(other.Value) 
                && Condition.Equals(other.Condition);
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
                var hashCode = Stat.GetHashCode();
                hashCode = (hashCode * 397) ^ Form.GetHashCode();
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                hashCode = (hashCode * 397) ^ Condition.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Stat: {Stat}\n  Form: {Form}\n  Value: {Value}\n  Condition: {Condition}";
        }
    }
}