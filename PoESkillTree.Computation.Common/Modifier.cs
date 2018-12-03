using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Represents a single parsed Modifier.
    /// </summary>
    public class Modifier : ValueObject
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

        protected override object ToTuple() => (WithSequenceEquality(Stats), Form, Value, Source);
    }
}