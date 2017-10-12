using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public interface IReferencedRegexes
    {
        bool ContainsReference(string referenceName);

        IEnumerable<string> GetRegexes(string referenceName);
    }
}