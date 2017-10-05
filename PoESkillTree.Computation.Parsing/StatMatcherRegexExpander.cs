using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing
{
    public class StatMatcherRegexExpander : IEnumerable<MatcherData>
    {
        public const string ValueRegex = @"\d+(?:\.\d+)?";
        public const string LeftDelimiterRegex = @"(?<=^|\s)";
        public const string RightDelimiterRegex = @"(?=$|\s)";

        private readonly IStatMatchers _statMatchers;
        private readonly Lazy<IReadOnlyList<MatcherData>> _expanded;

        private string _leftDelimiter;
        private string _rightDelimiter;

        public StatMatcherRegexExpander(IStatMatchers statMatchers)
        {
            _statMatchers = statMatchers;
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
            _leftDelimiter = _statMatchers.MatchesWholeLineOnly ? "^" : LeftDelimiterRegex;
            _rightDelimiter = _statMatchers.MatchesWholeLineOnly ? "$" : RightDelimiterRegex;
            return
                from data in _statMatchers.Matchers
                let regex = Expand(data.Regex)
                select new MatcherData(regex, data.ModifierBuilder, data.MatchSubstitution);
        }

        private string Expand(string regex)
        {
            var valueIndex = 0;
            return _leftDelimiter
                   + Regex.Replace(regex, "#", match => $@"(?<value{valueIndex++}>{ValueRegex})")
                   + _rightDelimiter;
        }
    }
}