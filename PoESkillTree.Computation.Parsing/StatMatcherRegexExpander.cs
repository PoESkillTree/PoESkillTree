using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing
{
    public class StatMatcherRegexExpander : IEnumerable<MatcherData>
    {
        public const string ValueRegex = @"\d+(\.\d+)?";
        public const string LeftDelimiterRegex = @"(?<=^|\s)";
        public const string RightDelimiterRegex = @"(?=$|\s)";

        private readonly IStatMatchers _statMatchers;
        private readonly IReferencedRegexes _referencedRegexes;
        private readonly Lazy<IReadOnlyList<MatcherData>> _expanded;

        public StatMatcherRegexExpander(IStatMatchers statMatchers, 
            IReferencedRegexes referencedRegexes)
        {
            _statMatchers = statMatchers;
            _referencedRegexes = referencedRegexes;
            _expanded = new Lazy<IReadOnlyList<MatcherData>>(() => Expand().ToList());
        }

        public IEnumerator<MatcherData> GetEnumerator()
        {
            return _expanded.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<MatcherData> Expand()
        {
            var leftDelimiter = _statMatchers.MatchesWholeLineOnly ? "^" : LeftDelimiterRegex;
            var rightDelimiter = _statMatchers.MatchesWholeLineOnly ? "$" : RightDelimiterRegex;
            return
                from data in _statMatchers
                let regex = Expand(data.Regex, leftDelimiter, rightDelimiter)
                select new MatcherData(regex, data.ModifierResult, data.MatchSubstitution);
        }

        private string Expand(string regex, string leftDelimiter, string rightDelimiter)
        {
            ValidateRegex(regex);
            return leftDelimiter
                   + ExpandReferences(ExpandValues(regex), "reference")
                   + rightDelimiter;
        }

        private void ValidateRegex(string regexString)
        {
            var regex = new Regex(regexString);
            foreach (var groupName in regex.GetGroupNames())
            {
                if (groupName.StartsWith("value") || groupName.StartsWith("reference"))
                {
                    throw new ParseException(
                        $"Regex {regexString} contains invalid group name {groupName}");
                }
            }

            foreach (Match match in ReferenceConstants.ReferenceRegex.Matches(regexString))
            {
                var referenceName = match.Groups[1].Value;
                if (!_referencedRegexes.ContainsReference(referenceName))
                {
                    throw new ParseException(
                        $"Regex {regexString} contains unknown reference {referenceName}");
                }
            }
        }

        private static string ExpandValues(string regex)
        {
            var valueIndex = 0;
            return Regex.Replace(regex, "#", match => $@"(?<value{valueIndex++}>{ValueRegex})");
        }

        private string ExpandReferences(string regex, string referencePrefix)
        {
            var referenceIndex = 0;
            return ReferenceConstants.ReferenceRegex.Replace(regex, match =>
            {
                var referenceName = match.Groups[1].Value;
                var prefix = referencePrefix + referenceIndex;
                referenceIndex++;
                var regexes = _referencedRegexes.GetRegexes(referenceName)
                    .Select((matcher, index) => (matcher, index))
                    .OrderByDescending(t => t.matcher.Length)
                    .Select(t => $"(?<{prefix}_{referenceName}_{t.index}>{ExpandReferences(t.matcher, prefix + "_")})");
                var joinedRegex = string.Join("|", regexes);
                return $@"({joinedRegex})";
            });
        }
    }
}