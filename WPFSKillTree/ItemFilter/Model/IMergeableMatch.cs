using System;

namespace PoESkillTree.ItemFilter.Model
{
    public interface IMergeableMatch
    {
        /// <summary>
        /// Determines whether this match complements another match.
        /// </summary>
        /// <example>
        /// "Quality = 10" complements both "Quality > 10" and "Quality &lt; 9".
        /// </example>
        /// <param name="match">A match to compare with this match.</param>
        /// <returns>true if this match complements the other match, false otherwise</returns>
        bool Complements(Match match);

        /// <summary>
        /// Merges another match into this match.
        /// </summary>
        /// <param name="match">A match to be merged with.</param>
        void Merge(Match match);

        /// <summary>
        /// Determines whether this match is a subset of another match.
        /// </summary>
        /// <example>
        /// "Quality = 10" subsets "Quality > 9", but doesn't subset "Quality > 10".
        /// </example>
        /// <param name="match">A match to compare with this match.</param>
        /// <returns>true if this match is a subset of the other match, false otherwise.</returns>
        bool Subsets(Match match);
    }
}
