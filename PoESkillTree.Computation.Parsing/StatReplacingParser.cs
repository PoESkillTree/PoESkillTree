using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing
{
    public class StatReplacingParser<TResult> : IParser<IReadOnlyList<TResult>>
    {
        private readonly IParser<TResult> _inner;

        private readonly IReadOnlyList<StatReplacerData> _statReplacerData;

        public StatReplacingParser(IParser<TResult> inner, 
            IReadOnlyList<StatReplacerData> statReplacerData)
        {
            _inner = inner;
            _statReplacerData = statReplacerData;
        }

        public bool TryParse(string stat, out string remaining, out IReadOnlyList<TResult> result)
        {
            var ret = true;
            var results = new List<TResult>();
            var remainings = new List<string>();
            foreach (var subStat in GetReplacements(stat))
            {
                ret &= _inner.TryParse(subStat, out var singleRemaining, out var singleResult);
                results.Add(singleResult);
                if (singleRemaining != string.Empty)
                {
                    remainings.Add(singleRemaining);
                }
            }
            result = results;
            remaining = string.Join("\n", remainings);
            return ret;
        }

        private IEnumerable<string> GetReplacements(string stat)
        {
            var allMatches =
                from data in _statReplacerData
                let match = Regex.Match(stat, data.OriginalStatRegex)
                where match.Success
                select data.Replacements.Select(match.Result);
            return allMatches
                .DefaultIfEmpty(new[] { stat })
                .First();
        }
    }
}