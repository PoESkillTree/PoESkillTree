using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Values;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public class RegexGroupService : IRegexGroupParser
    {
        // TODO This depends too heavily on how StatMatcherRegexExpander expands.
        //      Extract group naming into this class (implementing two interfaces)

        private readonly IValueBuilders _valueBuilders;

        public RegexGroupService(IValueBuilders valueBuilders)
        {
            _valueBuilders = valueBuilders;
        }

        public IEnumerable<IValueBuilder> ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "")
        {
            var fullPrefix = ValueGroupPrefix + groupPrefix;
            return
                from pair in groups
                let groupName = pair.Key
                where groupName.StartsWith(fullPrefix)
                let suffix = groupName.Substring(fullPrefix.Length)
                where suffix.Count(c => c == '_') == 0
                let value = double.Parse(pair.Value)
                select _valueBuilders.Create(value);
        }

        public IEnumerable<(string referenceName, int matcherIndex, string groupPrefix)> ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "")
        {
            var fullPrefix = ReferenceGroupPrefix + groupPrefix;
            return
                from groupName in groupNames
                where groupName.StartsWith(fullPrefix)
                let suffix = groupName.Substring(fullPrefix.Length)
                let parts = suffix.Split('_')
                where parts.Length == 3
                let nestedReferenceName = parts[1]
                let nestedMatcherIndex = TryGet(parts[2])
                where nestedMatcherIndex.HasValue
                let nestedGroupPrefix = groupPrefix + parts[0] + "_"
                select (nestedReferenceName, nestedMatcherIndex.Value, nestedGroupPrefix);
        }

        private static int? TryGet(string s)
        {
            return int.TryParse(s, out var r) ? (int?) r : null;
        }
    }
}