using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public class ReferenceManager : IReferencedRegexes, IReferenceToMatcherDataResolver
    {
        private readonly IReadOnlyList<IReferencedMatchers> _referencedMatchersList;
        private readonly IReadOnlyList<IStatMatchers> _statMatchersList;

        public ReferenceManager(IReadOnlyList<IReferencedMatchers> referencedMatchersList,
            IReadOnlyList<IStatMatchers> statMatchersList)
        {
            _referencedMatchersList = referencedMatchersList;
            _statMatchersList = statMatchersList;
        }

        public bool ContainsReference(string referenceName)
        {
            return _referencedMatchersList.Any(r => r.ReferenceName == referenceName) ||
                   _statMatchersList.Any(r => r.ReferenceNames.Contains(referenceName));
        }

        public IEnumerable<string> GetRegexes(string referenceName)
        {
            var referencedMatchers = _referencedMatchersList
                .FirstOrDefault(r => r.ReferenceName == referenceName);
            if (referencedMatchers != null)
            {
                return referencedMatchers.Select(d => d.Regex);
            }
            return _statMatchersList
                .Where(r => r.ReferenceNames.Contains(referenceName))
                .SelectMany(r => r.Select(d => d.Regex));
        }

        public bool TryGetReferencedMatcherData(
            string referenceName, int matcherIndex, out ReferencedMatcherData matcherData)
        {
            matcherData = _referencedMatchersList
                .Where(r => r.ReferenceName == referenceName)
                .Flatten()
                .ElementAtOrDefault(matcherIndex);
            return matcherData != null;
        }

        public bool TryGetMatcherData(string referenceName, int matcherIndex, out MatcherData matcherData)
        {
            matcherData = _statMatchersList
                .Where(r => r.ReferenceNames.Contains(referenceName))
                .Flatten()
                .ElementAtOrDefault(matcherIndex);
            return matcherData != null;
        }
    }
}