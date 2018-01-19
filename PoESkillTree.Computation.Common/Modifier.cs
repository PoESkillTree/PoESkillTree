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

        public override bool Equals(object obj) =>
            (obj == this) || (obj is Modifier other && Equals(other));

        private bool Equals(Modifier other) =>
            Stats.SequenceEqual(other.Stats) && Form.Equals(other.Form) && Value.Equals(other.Value);

        public override int GetHashCode() =>
            (Stats, Form, Value).GetHashCode();

        public override string ToString() =>
            $"Stats: {Stats}\n  Form: {Form}\n  Value: {Value}";
    }
}