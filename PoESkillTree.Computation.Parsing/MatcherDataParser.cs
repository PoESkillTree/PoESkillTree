using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// The leaf parser in the parser hierarchy. Parses stats by iterating through <see cref="MatcherData"/> instances
    /// to find the matching <see cref="MatcherData.Regex"/> with the longest match. The output remaining is
    /// the input stat but having the matched substring replaced by <see cref="MatcherData.MatchSubstitution"/>.
    /// The output result contains the matched <see cref="MatcherData"/>'s <see cref="MatcherData.Modifier"/> and
    /// the group names of <see cref="MatcherData.Regex"/> with their captured substrings.
    /// </summary>
    public class MatcherDataParser : IParser<MatcherDataParseResult>
    {
        private readonly IEnumerable<MatcherData> _matcherData;

        private readonly RegexCache _regexCache =
            new RegexCache(RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public MatcherDataParser(IEnumerable<MatcherData> matcherData)
        {
            _matcherData = matcherData;
        }

        public ParseResult<MatcherDataParseResult> Parse(string stat)
        {
            var xs =
                from matcherData in _matcherData
                let regex = _regexCache[matcherData.Regex]
                let match = regex.Match(stat)
                where match.Success
                orderby match.Length descending
                select new
                {
                    matcherData.Modifier,
                    Remaining = GetRemaining(matcherData, stat, match),
                    Groups = SelectGroups(regex, match.Groups)
                };

            var x = xs.FirstOrDefault();
            return x == null
                ? (successfullyParsed: false, remaining: stat, null)
                : (successfullyParsed: true, remaining: x.Remaining, new MatcherDataParseResult(x.Modifier, x.Groups));
        }

        private static string GetRemaining(MatcherData matcherData, string stat, Match match)
        {
            return stat.Substring(0, match.Index)
                   + match.Result(matcherData.MatchSubstitution)
                   + stat.Substring(match.Index + match.Length);
        }

        private static IReadOnlyDictionary<string, string> SelectGroups(Regex regex, GroupCollection groups)
        {
            return regex.GetGroupNames()
                .Where(gn => !string.IsNullOrEmpty(groups[gn].Value))
                .ToDictionary(gn => gn, gn => groups[gn].Value);
        }
    }
}