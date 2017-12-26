using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public class StatMatcherRegexExpander : IEnumerable<MatcherData>
    {
        public const string ValueRegex = @"\d+(\.\d+)?";
        public const string LeftDelimiterRegex = @"(?<=^|\s)";
        public const string RightDelimiterRegex = @"(?=$|\s)";

        private readonly IStatMatchers _statMatchers;
        private readonly IReferencedRegexes _referencedRegexes;
        private readonly IRegexGroupFactory _regexGroupFactory;
        private readonly Lazy<IReadOnlyList<MatcherData>> _expanded;

        public StatMatcherRegexExpander(
            IStatMatchers statMatchers, 
            IReferencedRegexes referencedRegexes,
            IRegexGroupFactory regexGroupFactory)
        {
            _statMatchers = statMatchers;
            _referencedRegexes = referencedRegexes;
            _regexGroupFactory = regexGroupFactory;
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
                   + ExpandReferences(ExpandValues(regex), "")
                   + rightDelimiter;
        }

        private void ValidateRegex(string regexString)
        {
            var regex = new Regex(regexString);
            foreach (var groupName in regex.GetGroupNames())
            {
                if (groupName.StartsWith(ReferenceConstants.ValueGroupPrefix) || groupName.StartsWith(ReferenceConstants.ReferenceGroupPrefix))
                {
                    throw new ParseException(
                        $"Regex {regexString} contains invalid group name {groupName}");
                }
            }

            foreach (Match match in ReferenceConstants.ReferencePlaceholderRegex.Matches(regexString))
            {
                var referenceName = match.Groups[1].Value;
                if (!_referencedRegexes.ContainsReference(referenceName))
                {
                    throw new ParseException(
                        $"Regex {regexString} contains unknown reference {referenceName}");
                }
            }
        }

        private string ExpandValues(string regex)
        {
            var valueIndex = 0;
            return ReferenceConstants.ValuePlaceholderRegex.Replace(regex, match =>
            {
                var prefix = valueIndex.ToString();
                valueIndex++;
                return _regexGroupFactory.CreateValueGroup(prefix, ValueRegex);
            });
        }

        private string ExpandReferences(string regex, string referencePrefix)
        {
            var referenceIndex = 0;
            return ReferenceConstants.ReferencePlaceholderRegex.Replace(regex, match =>
            {
                var referenceName = match.Groups[1].Value;
                var prefix = _regexGroupFactory.CombineGroupPrefixes(referencePrefix, referenceIndex.ToString());
                referenceIndex++;
                var indexedReferencedRegexes = _referencedRegexes.GetRegexes(referenceName)
                    .Select((matcher, index) => (matcher, index));
                var regexes =
                    from t in indexedReferencedRegexes
                    orderby t.matcher.Length descending
                    let innerRegex = ExpandReferences(t.matcher, prefix)
                    select _regexGroupFactory.CreateReferenceGroup(prefix, referenceName, t.index, innerRegex);
                var joinedRegex = string.Join("|", regexes);
                return $@"({joinedRegex})";
            });
        }
    }
}