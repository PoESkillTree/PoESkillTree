using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Represents a single parsed Modifier.
    /// </summary>
    public class Modifier
    {
        /// <summary>
        /// Defines the subgraphs of the calculation graph this modifier applies to.
        /// </summary>
        public IReadOnlyList<IStat> Stats { get; }

        /// <summary>
        /// Defines the form in which this modifier applies to stat subgraphs.
        /// </summary>
        public Form Form { get; }

        /// <summary>
        /// Defines the formula to calculate the value of this modifier.
        /// </summary>
        public IValue Value { get; }

        /// <summary>
        /// Defines the source of this modifier.
        /// </summary>
        public ModifierSource Source { get; }

        public Modifier(IReadOnlyList<IStat> stats, Form form, IValue value, ModifierSource source)
        {
            Stats = stats;
            Form = form;
            Value = value;
            Source = source;
        }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is Modifier other && Equals(other));

        private bool Equals(Modifier other) =>
            Stats.SequenceEqual(other.Stats) && Form.Equals(other.Form) && Value.Equals(other.Value)
            && Source.Equals(other.Source);

        public override int GetHashCode() =>
            (Stats.SequenceHash(), Form, Value, Source).GetHashCode();

        public override string ToString() =>
            $"Stats: {string.Join("    \n", Stats)}\n  Form: {Form}\n  Value: {Value}\n  Source: {Source}";
    }
}