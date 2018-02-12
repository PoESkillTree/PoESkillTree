using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Holds regex strings of references. Used to expand references into the regexes of their matchers.
    /// Conceptually similar to an <c>IReadOnlyDictionary&lt;string, IEnumerable&lt;string&gt;&gt;</c>.
    /// </summary>
    public interface IReferencedRegexes
    {
        /// <returns>The regex strings for the given reference. Empty if nothing for the given reference is stored.
        /// </returns>
        IEnumerable<string> GetRegexes(string referenceName);
    }
}