using System.Collections.Generic;

namespace PoESkillTree.Computation.Common.Data
{
    /// <summary>
    /// Data that specifies by which stat lines a stat line matching <see cref="OriginalStatRegex"/> should be replaced.
    /// </summary>
    public class StatReplacerData
    {
        /// <summary>
        /// The regex pattern the stat line must match to be replaced. The pattern must match the whole stat line,
        /// i.e. it must match <c>"^" + OriginalStatRegex + "$"</c>.
        /// </summary>
        /// <remarks>
        /// The regex pattern is used with the IgnoreCase and CultureInvariant options.
        /// </remarks>
        public string OriginalStatRegex { get; }

        /// <summary>
        /// The stat lines the original stat line is replaced by if <see cref="OriginalStatRegex"/> matches.
        /// </summary>
        public IReadOnlyList<string> Replacements { get; }

        public StatReplacerData(string originalStatRegex, IReadOnlyList<string> replacements)
        {
            OriginalStatRegex = originalStatRegex;
            Replacements = replacements;
        }
    }
}