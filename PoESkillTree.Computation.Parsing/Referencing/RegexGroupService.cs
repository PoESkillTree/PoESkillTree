using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils.Extensions;
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

        public IReadOnlyList<IValueBuilder> ParseValues(
            IReadOnlyDictionary<string, string> groups, string groupPrefix = "")
        {
            var fullPrefix = ValueGroupPrefix + groupPrefix;
            var values = new List<IValueBuilder>();
            foreach (var (groupName, stringValue) in groups)
            {
                if (!groupName.StartsWith(fullPrefix, StringComparison.Ordinal))
                    continue;

                var suffix = groupName.Substring(fullPrefix.Length);
                if (suffix.Count(c => c == GroupNamePartDelimiter) != 0)
                    continue; // ignore nested values

                var value = double.Parse(stringValue);
                values.Add(_valueBuilders.Create(value));
            }
            return values;
        }

        public IReadOnlyList<(string referenceName, int matcherIndex, string groupPrefix)> ParseReferences(
            IEnumerable<string> groupNames, string groupPrefix = "")
        {
            var fullPrefix = ReferenceGroupPrefix + groupPrefix;
            var references = new List<(string, int, string)>();
            foreach (var groupName in groupNames)
            {
                if (!groupName.StartsWith(fullPrefix, StringComparison.Ordinal))
                    continue;

                var suffix = groupName.Substring(fullPrefix.Length);
                var parts = suffix.Split(GroupNamePartDelimiter);
                if (parts.Length != 3)
                    continue; // Ignore nested values

                var nestedMatcherIndex = TryGet(parts[2]);
                if (!nestedMatcherIndex.HasValue)
                    continue;

                var nestedReferenceName = parts[1];
                var nestedGroupPrefix = groupPrefix + parts[0] + GroupNamePartDelimiter;
                references.Add((nestedReferenceName, nestedMatcherIndex.Value, nestedGroupPrefix));
            }
            return references;
        }

        private static int? TryGet(string s)
            => int.TryParse(s, out var r) ? (int?) r : null;
    }
}