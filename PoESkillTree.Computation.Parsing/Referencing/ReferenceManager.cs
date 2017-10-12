using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Data;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public class ReferenceManager : IReferencedRegexes
    {
        private readonly IReadOnlyList<IReferencedMatchers> _referencedMatchersList;
        private readonly IReadOnlyList<IStatMatchers> _statMatchersList;

        public ReferenceManager(IReadOnlyList<IReferencedMatchers> referencedMatchersList,
            IReadOnlyList<IStatMatchers> statMatchersList)
        {
            _referencedMatchersList = referencedMatchersList;
            _statMatchersList = statMatchersList;
        }

        public void Validate()
        {
            var knownReferences = ValidateReferencedMatchersList();
            var recursiveReferences = ValidateStatMatchersList(knownReferences);
            ValidateReferences(knownReferences, recursiveReferences);
        }

        private ISet<string> ValidateReferencedMatchersList()
        {
            var knownReferences = new HashSet<string>();
            foreach (var referencedMatchers in _referencedMatchersList)
            {
                var referenceName = referencedMatchers.ReferenceName;
                var regexes = referencedMatchers.Select(d => d.Regex).ToList();
                if (regexes.Any(s => s.Contains("#")))
                {
                    throw new ParseException(
                        $"A regex of reference {referenceName} contains values");
                }
                if (regexes.Any(s => ReferenceRegex.IsMatch(s)))
                {
                    throw new ParseException(
                        $"A regex of reference {referenceName} contains references");
                }
                if (!knownReferences.Add(referenceName))
                {
                    throw new ParseException(
                        $"The reference name {referenceName} is not unique between the IReferencedMatchers");
                }
            }
            return knownReferences;
        }

        private IReadOnlyDictionary<string, ISet<string>> ValidateStatMatchersList(
            ISet<string> knownReferences)
        {
            var recursiveReferences = new Dictionary<string, ISet<string>>();
            foreach (var statMatchers in _statMatchersList)
            {
                var referenceNames = statMatchers.ReferenceNames;
                if (referenceNames.IsEmpty())
                {
                    continue;
                }
                var regexes = statMatchers.Select(d => d.Regex).ToList();
                if (regexes.Any(s => s.Contains("#")))
                {
                    throw new ParseException(
                        $"A regex of reference {string.Join(",", referenceNames)} contains values");
                }
                foreach (var referenceName in referenceNames)
                {
                    if (knownReferences.Contains(referenceName))
                    {
                        throw new ParseException(
                            $"The reference name {referenceName} is used by both an IReferencedMatchers and an IStatMatchers");
                    }
                    var containedReferences = regexes
                        .SelectMany(r => ReferenceRegex.Matches(r).Cast<Match>())
                        .Select(m => m.Groups[1].Value);
                    recursiveReferences.GetOrAdd(referenceName, _ => new HashSet<string>())
                        .UnionWith(containedReferences);
                }
            }
            knownReferences.UnionWith(recursiveReferences.Keys);
            return recursiveReferences;
        }

        private void ValidateReferences(ISet<string> knownReferences, 
            IReadOnlyDictionary<string, ISet<string>> recursiveReferences)
        {
            foreach (var (key, referenced) in recursiveReferences)
            {
                foreach (var reference in referenced)
                {
                    if (!knownReferences.Contains(reference))
                    {
                        throw new ParseException(
                            $"Unknown reference {reference} referenced by {key}");
                    }
                }
                var unvisited = new HashSet<string>(referenced);
                while (unvisited.Any())
                {
                    var current = unvisited.First();
                    unvisited.Remove(current);
                    if (key == current)
                    {
                        throw new ParseException($"{key} recursively references itself");
                    }
                    if (recursiveReferences.TryGetValue(current, out var newUnvisited))
                    {
                        unvisited.UnionWith(newUnvisited);
                    }
                }
            }
        }

        // TODO caching for ContainsReferences and GetRegexes?

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
    }
}