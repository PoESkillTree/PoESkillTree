using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Common.Parsing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// Implementation of both <see cref="IRegexGroupFactory"/> and <see cref="IRegexGroupParser"/> to keep everything
    /// related to regex group naming in one place (plus <see cref="Common.Parsing.ReferenceConstants"/>).
    /// </summary>
    public class RegexGroupService : IRegexGroupParser, IRegexGroupFactory
    {
        private const char GroupNamePartDelimiter = '_';

        private readonly IValueBuilders _valueBuilders;

        public RegexGroupService(IValueBuilders valueBuilders)
        {
            _valueBuilders = valueBuilders;
        }

        public string CreateValueGroup(string groupPrefix, string innerRegex)
        {
            if (innerRegex.IsEmpty())
                throw new ArgumentException("must not be empty", nameof(innerRegex));

            return $"(?<{ValueGroupPrefix}{groupPrefix}>{innerRegex})";
        }

        public string CreateReferenceGroup(
            string groupPrefix, string referenceName, int matcherIndex, string innerRegex)
        {
            if (innerRegex.IsEmpty())
                throw new ArgumentException("must not be empty", nameof(innerRegex));
            if (referenceName.IsEmpty())
                throw new ArgumentException("must not be empty", nameof(referenceName));

            return "(?<" + ReferenceGroupPrefix + groupPrefix
                   + GroupNamePartDelimiter + referenceName
                   + GroupNamePartDelimiter + matcherIndex
                   + ">" + innerRegex + ")";
        }

        public string CombineGroupPrefixes(string left, string right)
        {
            if (right.IsEmpty())
                throw new ArgumentException("must not be empty", nameof(right));
            if (left.IsEmpty())
                return right;

            return left + GroupNamePartDelimiter + right;
        }

        public IEnumerable<IValueBuilder> ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "")
        {
            var fullPrefix = ValueGroupPrefix + groupPrefix;
            return
                from pair in groups
                let groupName = pair.Key
                where groupName.StartsWith(fullPrefix, StringComparison.Ordinal)
                let suffix = groupName.Substring(fullPrefix.Length)
                where suffix.Count(c => c == GroupNamePartDelimiter) == 0 // Ignore nested values
                let value = double.Parse(pair.Value)
                select _valueBuilders.Create(value);
        }

        public IEnumerable<(string referenceName, int matcherIndex, string groupPrefix)> ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "")
        {
            var fullPrefix = ReferenceGroupPrefix + groupPrefix;
            return
                from groupName in groupNames
                where groupName.StartsWith(fullPrefix, StringComparison.Ordinal)
                let suffix = groupName.Substring(fullPrefix.Length)
                let parts = suffix.Split(GroupNamePartDelimiter)
                where parts.Length == 3 // Ignore nested values
                let nestedReferenceName = parts[1]
                let nestedMatcherIndex = TryGet(parts[2])
                where nestedMatcherIndex.HasValue
                let nestedGroupPrefix = groupPrefix + parts[0] + GroupNamePartDelimiter
                select (nestedReferenceName, nestedMatcherIndex.Value, nestedGroupPrefix);
        }

        private static int? TryGet(string s)
        {
            return int.TryParse(s, out var r) ? (int?) r : null;
        }
    }
}