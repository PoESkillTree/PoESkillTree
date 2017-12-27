using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Data;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    /// <summary>
    /// <c>IEnumerable&lt;MatcherData&gt;</c> with expanded <see cref="MatcherData.Regex"/> values. Takes the
    /// <see cref="MatcherData"/> of an <see cref="IStatMatchers"/> instance and expands them (potentially recursively)
    /// using <see cref="IReferencedRegexes"/> and <see cref="IRegexGroupFactory"/>.
    /// </summary>
    public class StatMatcherRegexExpander : IEnumerable<MatcherData>
    {
        /// <summary>
        /// The pattern <see cref="ReferenceConstants.ValuePlaceholder"/> is expanded to.
        /// </summary>
        public const string ValueRegex = @"\d+(\.\d+)?";
        // Matches must have whitespace left and right of them, or the input string must end there.
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
                let regex = leftDelimiter
                            + ExpandReferences(ExpandValues(data.Regex), "")
                            + rightDelimiter
                select new MatcherData(regex, data.Modifier, data.MatchSubstitution);
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