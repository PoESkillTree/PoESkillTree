using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that potentially splits the input stat into multiple using a list of
    /// <see cref="StatReplacerData"/>, passes each of those stats to the decorated parser and outputs all results.
    /// <para>The output remaining is created by joining all stats' remaining outputs that are not only whitespace
    /// with newlines.</para>
    /// <para>Parsing is successful if all stats could be parsed successfully.</para>
    /// </summary>
    /// <typeparam name="TResult">Type of the decorated parser's results</typeparam>
    public class StatReplacingParser<TResult> : IStringParser<IReadOnlyList<TResult>>
    {
        private readonly IStringParser<TResult> _inner;

        private readonly Lazy<IReadOnlyList<(StatReplacerData data, Regex regex)>> _dataWithRegexes;

        public StatReplacingParser(IStringParser<TResult> inner,
            IReadOnlyList<StatReplacerData> statReplacerData)
        {
            _inner = inner;
            _dataWithRegexes = new Lazy<IReadOnlyList<(StatReplacerData, Regex)>>(
                () => statReplacerData.Select(d => (d, CreateRegex(d))).ToList());
        }

        private static Regex CreateRegex(StatReplacerData data)
            => new Regex("^" + data.OriginalStatRegex + "$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public StringParseResult<IReadOnlyList<TResult>> Parse(CoreParserParameter parameter)
        {
            var successfullyParsed = true;
            var remainings = new List<string>();
            var results = new List<TResult>();
            foreach (var replacementStat in GetReplacements(parameter.ModifierLine))
            {
                var (innerSuccess, innerRemaining, innerResult) = _inner.Parse(replacementStat, parameter);
                successfullyParsed &= innerSuccess;
                results.Add(innerResult);
                if (!string.IsNullOrWhiteSpace(innerRemaining))
                {
                    remainings.Add(innerRemaining);
                }
            }

            return (successfullyParsed, string.Join("\n", remainings), results);
        }

        private IEnumerable<string> GetReplacements(string stat)
        {
            IEnumerable<IEnumerable<string>> allMatches =
                from tuple in _dataWithRegexes.Value
                let match = tuple.regex.Match(stat)
                where match.Success
                select tuple.data.Replacements.Select(match.Result);
            return allMatches
                .DefaultIfEmpty(new[] { stat })
                .First();
        }
    }
}