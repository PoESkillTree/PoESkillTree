using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// The leaf parser in the parser hierarchy. Parses stats by iterating through <see cref="MatcherData"/> instances
    /// to find the matching <see cref="MatcherData.Regex"/> with the longest match. The output remaining is
    /// the input stat but having the matched substring replaced by <see cref="MatcherData.MatchSubstitution"/>.
    /// The output result contains the matched <see cref="MatcherData"/>'s <see cref="MatcherData.Modifier"/> and
    /// the group names of <see cref="MatcherData.Regex"/> with their captured substrings.
    /// </summary>
    public class MatcherDataParser : IStringParser<MatcherDataParseResult>
    {
        private readonly IReadOnlyList<(MatcherData data, Regex regex)> _dataWithRegexes;

        public static MatcherDataParser Create(
            IReadOnlyList<MatcherData> matcherData, Func<string, string> matcherDataExpander)
            => new MatcherDataParser(CreateDataWithRegexes(matcherData, matcherDataExpander));

        private MatcherDataParser(IReadOnlyList<(MatcherData data, Regex regex)> dataWithRegexes)
            => _dataWithRegexes = dataWithRegexes;

        private static IReadOnlyList<(MatcherData data, Regex regex)> CreateDataWithRegexes(
            IReadOnlyList<MatcherData> data, Func<string, string> matcherDataExpander)
        {
            var list = new List<(MatcherData, Regex)>(data.Count);
            foreach (var d in data)
            {
                var regex = CreateRegex(matcherDataExpander(d.Regex));
                list.Add((d, regex));
            }
            return list;
        }

        private static Regex CreateRegex(string regex)
            => new Regex(regex,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        public StringParseResult<MatcherDataParseResult> Parse(CoreParserParameter parameter)
        {
            var stat = parameter.ModifierLine;
            Match longestMatch = null;
            MatcherData matchingData = null;
            Regex matchingRegex = null;
            foreach (var (data, regex) in _dataWithRegexes)
            {
                var match = regex.Match(stat);
                if (match.Success && (longestMatch is null || match.Length > longestMatch.Length))
                {
                    longestMatch = match;
                    matchingData = data;
                    matchingRegex = regex;
                }
            }

            if (longestMatch is null)
                return (false, stat, null);

            return (true,
                GetRemaining(matchingData, stat, longestMatch),
                new MatcherDataParseResult(matchingData.Modifier, SelectGroups(matchingRegex, longestMatch.Groups)));
        }

        private static string GetRemaining(MatcherData matcherData, string stat, Match match)
            => stat.Substring(0, match.Index)
               + match.Result(matcherData.MatchSubstitution)
               + stat.Substring(match.Index + match.Length);

        private static IReadOnlyDictionary<string, string> SelectGroups(Regex regex, GroupCollection groups)
            => regex.GetGroupNames()
                .Where(gn => !string.IsNullOrEmpty(groups[gn].Value))
                .ToDictionary(gn => gn, gn => groups[gn].Value);
    }
}