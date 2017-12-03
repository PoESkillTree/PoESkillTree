using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public interface IRegexGroupParser
    {
        IEnumerable<IValueBuilder> ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "");

        IEnumerable<(string referenceName, int matcherIndex, string groupPrefix)> ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "");
    }
}