using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Resolves references names with a matcher index back to the referenced matcher data. The indexes refer
    /// to the enumerable returned by <see cref="IReferencedRegexes.GetRegexes"/>.
    /// </summary>
    public interface IReferenceToMatcherDataResolver
    {
        /// <summary>
        /// Tries to get the <see cref="ReferencedMatcherData"/> with the given reference name at the given index.
        /// Returns false if the reference name does not exist, does not hold <see cref="ReferencedMatcherData"/> or
        /// has no data at the given index.
        /// </summary>
        bool TryGetReferencedMatcherData(
            string referenceName, int matcherIndex, out ReferencedMatcherData matcherData);

        /// <summary>
        /// Tries to get the <see cref="MatcherData"/> with the given reference name at the given index.
        /// Returns false if the reference name does not exist, does not hold <see cref="MatcherData"/> or
        /// has no data at the given index.
        /// </summary>
        bool TryGetMatcherData(string referenceName, int matcherIndex, out MatcherData matcherData);
    }
}