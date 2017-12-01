using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing
{
    public class MatcherDataParser : IParser<MatcherDataParseResult>
    {
        private readonly IEnumerable<MatcherData> _statMatchers;

        public MatcherDataParser(IEnumerable<MatcherData> statMatchers)
        {
            _statMatchers = statMatchers;
        }

        public bool TryParse(string stat, out string remaining, out MatcherDataParseResult result)
        {
            var xs =
                from m in _statMatchers
                let regex = CreateRegex(m.Regex)
                let match = regex.Match(stat)
                where match.Success
                orderby match.Length descending
                let replaced = stat.Substring(0, match.Index)
                               + match.Result(m.MatchSubstitution)
                               + stat.Substring(match.Index + match.Length)
                select new { m.ModifierResult, Result = replaced, Groups = SelectGroups(regex, match.Groups) };

            var x = xs.FirstOrDefault();
            if (x == null)
            {
                result = null;
                remaining = stat;
                return false;
            }
            result = new MatcherDataParseResult(x.ModifierResult, x.Groups);
            remaining = x.Result;
            return true;
        }

        private static Regex CreateRegex(string regex)
        {
            return new Regex(regex,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }

        private static IReadOnlyDictionary<string, string> SelectGroups(Regex regex, GroupCollection groups)
        {
            return regex.GetGroupNames()
                .Where(gn => !string.IsNullOrEmpty(groups[gn].Value))
                .ToDictionary(gn => gn, gn => groups[gn].Value);
        }
    }
}