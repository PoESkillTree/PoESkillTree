using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Data object for the return value of <see cref="IParser{TParameter}.Parse"/>.
    /// </summary>
    public class ParseResult
    {
        public ParseResult(bool successfullyParsed, IReadOnlyList<string> failedLines,
            IReadOnlyList<string> remainingSubstrings, IReadOnlyList<Modifier> modifiers)
            => (SuccessfullyParsed, FailedLines, RemainingSubstrings, Modifiers) =
                (successfullyParsed, failedLines, remainingSubstrings, modifiers);

        public void Deconstruct(out bool successfullyParsed, out IReadOnlyList<string> failedLines,
            out IReadOnlyList<string> remainingSubstrings, out IReadOnlyList<Modifier> modifiers)
            => (successfullyParsed, failedLines, remainingSubstrings, modifiers) =
                (SuccessfullyParsed, FailedLines, RemainingSubstrings, Modifiers);

        /// <summary>
        /// True if all modifiers were parsed successfully.
        /// </summary>
        public bool SuccessfullyParsed { get; }

        /// <summary>
        /// The modifier lines that were not parsed successfully.
        /// </summary>
        public IReadOnlyList<string> FailedLines { get; }

        /// <summary>
        /// For each modifier line that was not parsed successfully, the remaining substring that was not parsed at all.
        /// </summary>
        public IReadOnlyList<string> RemainingSubstrings { get; }

        /// <summary>
        /// The parsed modifiers. Empty if <see cref="SuccessfullyParsed"/> is false.
        /// </summary>
        public IReadOnlyList<Modifier> Modifiers { get; }
    }
}