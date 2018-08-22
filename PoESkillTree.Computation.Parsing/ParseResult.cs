using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Data object for the return value of <see cref="IParser{TParameter}.Parse"/>.
    /// </summary>
    public class ParseResult
    {
        public static ParseResult Success(IReadOnlyList<Modifier> modifiers)
            => new ParseResult(new string[0], new string[0], modifiers);

        public static ParseResult Failure(string modifierLine, string remainingSubstring)
            => new ParseResult(new[] { modifierLine }, new[] { remainingSubstring }, new Modifier[0]);

        private ParseResult(IReadOnlyList<string> failedLines,
            IReadOnlyList<string> remainingSubstrings, IReadOnlyList<Modifier> modifiers)
            => (FailedLines, RemainingSubstrings, Modifiers) = (failedLines, remainingSubstrings, modifiers);

        public void Deconstruct(out IReadOnlyList<string> failedLines,
            out IReadOnlyList<string> remainingSubstrings, out IReadOnlyList<Modifier> modifiers)
            => (failedLines, remainingSubstrings, modifiers) = (FailedLines, RemainingSubstrings, Modifiers);

        /// <summary>
        /// True if all modifier lines were parsed successfully.
        /// </summary>
        public bool SuccessfullyParsed => FailedLines.IsEmpty();

        /// <summary>
        /// The modifier lines that were not parsed successfully.
        /// </summary>
        public IReadOnlyList<string> FailedLines { get; }

        /// <summary>
        /// For each modifier line that was not parsed successfully, the remaining substring that was not parsed at all.
        /// </summary>
        public IReadOnlyList<string> RemainingSubstrings { get; }

        /// <summary>
        /// The parsed modifiers. Does not contain any modifiers for <see cref="FailedLines"/>.
        /// </summary>
        public IReadOnlyList<Modifier> Modifiers { get; }

        public static ParseResult Aggregate(IEnumerable<ParseResult> results)
        {
            var failedLines = new List<string>();
            var remainingSubstrings = new List<string>();
            var modifiers = new List<Modifier>();
            foreach (var (f, r, m) in results)
            {
                failedLines.AddRange(f);
                remainingSubstrings.AddRange(r);
                modifiers.AddRange(m);
            }
            return new ParseResult(failedLines, remainingSubstrings, modifiers);
        }
    }
}