namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Creates regex groups to use when creating valid expanded regex strings from
    /// <see cref="Data.MatcherData.Regex"/> values.
    /// </summary>
    /// <remarks>
    /// <see cref="IRegexGroupParser"/> parses groups created by <see cref="IRegexGroupFactory"/> instances.
    /// </remarks>
    public interface IRegexGroupFactory
    {
        /// <summary>
        /// Creates and returns a regex group that matches values.
        /// </summary>
        /// <param name="groupPrefix">Prefix of the group name that is unique within the whole regex.</param>
        /// <param name="innerRegex">The inner regex of the group.</param>
        string CreateValueGroup(string groupPrefix, string innerRegex);

        /// <summary>
        /// Creates and returns a regex group that matches an expanded reference.
        /// </summary>
        /// <param name="groupPrefix">Prefix of the group name that is unique within the whole regex.</param>
        /// <param name="referenceName">The name of the reference. This will be included in the group's name.</param>
        /// <param name="matcherIndex">The index of the expanded matcher in all matchers of the given reference.
        /// This will be included in the group's name.</param>
        /// <param name="innerRegex">The inner regex of the group.</param>
        /// <remarks>
        /// <paramref name="referenceName"/> and <paramref name="matcherIndex"/> are the same as used in
        /// <see cref="IReferencedRegexes"/> and <see cref="IReferenceToMatcherDataResolver"/>.
        /// </remarks>
        string CreateReferenceGroup(string groupPrefix, string referenceName, int matcherIndex, string innerRegex);

        /// <summary>
        /// Merges two group prefixes as used in <see cref="CreateValueGroup"/> and <see cref="CreateReferenceGroup"/>
        /// to represent a nesting level that is the sum of the nesting levels of both prefixes (if they were itself
        /// created by this method).
        /// </summary>
        string CombineGroupPrefixes(string left, string right);
    }
}