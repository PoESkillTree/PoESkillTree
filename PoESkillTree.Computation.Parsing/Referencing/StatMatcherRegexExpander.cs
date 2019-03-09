using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Data;
using static PoESkillTree.Computation.Common.Parsing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// <c>IEnumerable&lt;MatcherData&gt;</c> with expanded <see cref="MatcherData.Regex"/> values. Takes the
    /// <see cref="MatcherData"/> of an <see cref="IStatMatchers"/> instance and expands them (potentially recursively)
    /// using <see cref="IReferencedRegexes"/> and <see cref="IRegexGroupFactory"/>.
    /// </summary>
    public class StatMatcherRegexExpander
    {
        /// <summary>
        /// The pattern <see cref="ValuePlaceholder"/> is expanded to.
        /// </summary>
        public const string ValueRegex = @"-?\d+(\.\d+)?";
        // Matches must have whitespace left of them if the input string does not start.
        public const string LeftDelimiterRegex = @"(?<=^|\s)";
        // Matches must have whitespace right of them if the input string does not end.
        // A ',' at the end is always allowed.
        public const string RightDelimiterRegex = @",?(?=$|\s)";

        private readonly IReferencedRegexes _referencedRegexes;
        private readonly IRegexGroupFactory _regexGroupFactory;
        private readonly bool _matchesWholeLineOnly;

        public StatMatcherRegexExpander(
            IReferencedRegexes referencedRegexes,
            IRegexGroupFactory regexGroupFactory,
            bool matchesWholeLineOnly)
        {
            _referencedRegexes = referencedRegexes;
            _regexGroupFactory = regexGroupFactory;
            _matchesWholeLineOnly = matchesWholeLineOnly;
        }

        public string Expand(string regex)
        {
            var leftDelimiter = _matchesWholeLineOnly ? "^" : LeftDelimiterRegex;
            var rightDelimiter = _matchesWholeLineOnly ? "$" : RightDelimiterRegex;
            return leftDelimiter
                   + ExpandReferences(ExpandValues(regex), "")
                   + rightDelimiter;
        }

        private string ExpandValues(string regex)
        {
            var valueIndex = 0;
            return ValuePlaceholderRegex.Replace(regex, match =>
            {
                var prefix = valueIndex.ToString();
                valueIndex++;
                return _regexGroupFactory.CreateValueGroup(prefix, ValueRegex);
            });
        }

        private string ExpandReferences(string regex, string referencePrefix)
        {
            var referenceIndex = 0;
            return ReferencePlaceholderRegex.Replace(regex, match =>
            {
                var referenceName = match.Groups[1].Value;
                var prefix = _regexGroupFactory.CombineGroupPrefixes(referencePrefix, referenceIndex.ToString());
                referenceIndex++;
                var indexedReferencedRegexes = _referencedRegexes.GetRegexes(referenceName)
                    .Select((matcher, index) => (matcher, index));
                IEnumerable<string> regexes =
                    from t in indexedReferencedRegexes
                    // Without ordering, e.g. "Has a" in "(Has|Has a)" would never be matched
                    orderby t.matcher.Length descending
                    // Recursive expansion
                    let innerRegex = ExpandReferences(t.matcher, prefix)
                    select _regexGroupFactory.CreateReferenceGroup(prefix, referenceName, t.index, innerRegex);
                var joinedRegex = string.Join("|", regexes);
                return $"({joinedRegex})";
            });
        }
    }
}