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
        private readonly Lazy<IReadOnlyList<(MatcherData data, Regex regex)>> _dataWithRegexes;

        public MatcherDataParser(
            IReadOnlyList<MatcherData> matcherData, Func<string, string> matcherDataExpander)
        {
            _dataWithRegexes = new Lazy<IReadOnlyList<(MatcherData, Regex)>>(
                () => CreateDataWithRegexes(matcherData, matcherDataExpander));
        }

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
            var xs =
                from tuple in _dataWithRegexes.Value
                let match = tuple.regex.Match(stat)
                where match.Success
                orderby match.Length descending
                select (
                    successfullyParsed: true,
                    remaining: GetRemaining(tuple.data, stat, match),
                    new MatcherDataParseResult(tuple.data.Modifier, SelectGroups(tuple.regex, match.Groups))
                );

            return xs.DefaultIfEmpty((false, stat, (MatcherDataParseResult) null)).First();
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