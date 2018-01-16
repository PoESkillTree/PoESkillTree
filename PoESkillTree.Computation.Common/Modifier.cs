using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Represents a single parsed Modifier including a Stat, Form and Value.
    /// </summary>
    public class Modifier
    {
        public IReadOnlyList<IStat> Stats { get; }

        public Form Form { get; }

        public IValue Value { get; }

        public Modifier(IReadOnlyList<IStat> stats, Form form, IValue value)
        {
            Stats = stats;
            Form = form;
            Value = value;
        }

        private bool Equals(Modifier other)
        {
            return Stats.SequenceEqual(other.Stats) 
                   && Form.Equals(other.Form) 
                   && Value.Equals(other.Value);
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
                var hashCode = Stats.GetHashCode();
                hashCode = (hashCode * 397) ^ Form.GetHashCode();
                hashCode = (hashCode * 397) ^ Value.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"Stats: {Stats}\n  Form: {Form}\n  Value: {Value}";
        }
    }
}