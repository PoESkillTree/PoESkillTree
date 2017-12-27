using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Parses matched regex groups into values or back into the information about references that allows resolving 
    /// them using <see cref="IReferenceToMatcherDataResolver"/>.
    /// </summary>
    /// <remarks>
    /// Regex groups must have been created by a corresponding <see cref="IRegexGroupFactory"/> instance.
    /// </remarks>
    public interface IRegexGroupParser
    {
        /// <summary>
        /// Parses the given regex groups into values.
        /// </summary>
        /// <param name="groups">Dictionary of group names and their captured match substrings (from which values are 
        /// parsed).</param>
        /// <param name="groupPrefix">The prefix group names must have to be parsed. Group names must also be on
        /// the nesting level given by the prefix to be parsed, e.g. not nested with the empty prefix.</param>
        /// <returns>The parsed values.</returns>
        IEnumerable<IValueBuilder> ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "");

        /// <summary>
        /// Parses the given regex group names into reference names and matcher indices.
        /// </summary>
        /// <param name="groupNames">The names of all regex groups that were matched.</param>
        /// <param name="groupPrefix">The prefix group names must have to be parsed. Group names must also be on
        /// the nesting level given by the prefix to be parsed, e.g. not nested with the empty prefix.</param>
        /// <returns>A tuple for each matched reference consisting of the reference name and matcher index resolvable
        /// using <see cref="IReferenceToMatcherDataResolver"/>, and the group prefix of reference groups nested
        /// into the reference. These may recursively be parsed by another call to this method.</returns>
        IEnumerable<(string referenceName, int matcherIndex, string groupPrefix)> ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "");
    }
}