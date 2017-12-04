using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Data;
using static PoESkillTree.Computation.Parsing.Referencing.ReferenceConstants;

namespace PoESkillTree.Computation.Parsing.Referencing
{
    public static class ReferenceValidator
    {
        public static void Validate(IReadOnlyList<IReferencedMatchers> referencedMatchersList,
            IReadOnlyList<IStatMatchers> statMatchersList)
        {
            var knownReferences = ValidateReferencedMatchersList(referencedMatchersList);
            var recursiveReferences = ValidateStatMatchersList(statMatchersList, knownReferences);
            ValidateReferences(knownReferences, recursiveReferences);
        }

        private static ISet<string> ValidateReferencedMatchersList(
            IReadOnlyList<IReferencedMatchers> referencedMatchersList)
        {
            var knownReferences = new HashSet<string>();
            foreach (var referencedMatchers in referencedMatchersList)
            {
                var referenceName = referencedMatchers.ReferenceName;
                var regexes = referencedMatchers.Select(d => d.Regex).ToList();
                if (regexes.Any(s => s.Contains(ValuePlaceholder)))
                {
                    throw new ParseException(
                        $"A regex of reference {referenceName} contains values");
                }
                if (regexes.Any(s => ReferencePlaceholderRegex.IsMatch(s)))
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

        private static IReadOnlyDictionary<string, ISet<string>> ValidateStatMatchersList(
            IReadOnlyList<IStatMatchers> statMatchersList, ISet<string> knownReferences)
        {
            var recursiveReferences = new Dictionary<string, ISet<string>>();
            foreach (var statMatchers in statMatchersList)
            {
                var referenceNames = statMatchers.ReferenceNames;
                if (referenceNames.IsEmpty())
                {
                    continue;
                }
                var regexes = statMatchers.Select(d => d.Regex).ToList();
                if (regexes.Any(s => s.Contains(ValuePlaceholder)))
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
                        .SelectMany(r => ReferencePlaceholderRegex.Matches(r).Cast<Match>())
                        .Select(m => m.Groups[1].Value);
                    recursiveReferences.GetOrAdd(referenceName, _ => new HashSet<string>())
                        .UnionWith(containedReferences);
                }
            }
            knownReferences.UnionWith(recursiveReferences.Keys);
            return recursiveReferences;
        }

        private static void ValidateReferences(ISet<string> knownReferences,
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
    }
}