using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing
{
    // TODO tests
    // TODO split into multiple classes? (probably ModifierResultToReferencedBuilder, maybe ResolveNested, maybe Resolve)
    public class ResolvingParser : IParser<IModifierBuilder>
    {
        private readonly IParser<MatcherDataParser.Result> _innerParser;
        private readonly IReferenceToMatcherDataResolver _referenceManager;
        private readonly IValueBuilders _valueBuilders;

        public ResolvingParser(
            IParser<MatcherDataParser.Result> innerParser, 
            IReferenceToMatcherDataResolver referenceManager,
            IValueBuilders valueBuilders)
        {
            _innerParser = innerParser;
            _referenceManager = referenceManager;
            _valueBuilders = valueBuilders;
        }

        public bool TryParse(string stat, out string remaining, out IModifierBuilder result)
        {
            if (!_innerParser.TryParse(stat, out remaining, out var innerResult))
            {
                result = innerResult?.ModifierBuilder;
                return false;
            }
            var context = CreateRootContext(innerResult.Groups);
            result = Resolve(innerResult.ModifierBuilder, context);
            return true;
        }

        private ResolveContext CreateRootContext(IReadOnlyDictionary<string, string> groups)
        {
            var values =
                from pair in groups
                let groupName = pair.Key
                where groupName.StartsWith("value")
                let value = int.Parse(pair.Value)
                select _valueBuilders.Create(value);
            var valueContext = new ResolvedMatchContext<IValueBuilder>(values.ToList());

            var groupNames = groups.Keys.ToList();
            var references =
                from groupName in groupNames
                where groupName.StartsWith("reference")
                let parts = groupName.Split('_')
                where parts.Length == 3
                let referenceName = parts[1]
                let matcherIndex = int.Parse(parts[2])
                let referencePrefix = parts[0] + "_"
                select ResolveNested(referenceName, matcherIndex, referencePrefix, groupNames);
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(references.ToList());

            return new ResolveContext(valueContext, referenceContext);
        }

        private ResolveContext CreateNestedContext(IReadOnlyList<string> groupNames, string referencePrefix)
        {
            var valueContext = new ResolvedMatchContext<IValueBuilder>(new IValueBuilder[0]);

            var nestedReferences =
                from groupName in groupNames
                where groupName.StartsWith(referencePrefix)
                let suffix = groupName.Substring(referencePrefix.Length)
                let parts = suffix.Split('_')
                where parts.Length == 3
                let nestedReferenceName = parts[1]
                let nestedMatcherIndex = int.Parse(parts[2])
                let nestedReferencePrefix = referencePrefix + parts[0] + "_"
                select ResolveNested(nestedReferenceName, nestedMatcherIndex, nestedReferencePrefix, groupNames);
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(nestedReferences.ToList());

            return new ResolveContext(valueContext, referenceContext);
        }

        private IReferenceConverter ResolveNested(
            string referenceName,
            int matcherIndex,
            string referencePrefix,
            IReadOnlyList<string> groupNames)
        {
            if (_referenceManager.TryGetReferencedMatcherData(referenceName, matcherIndex, out var referencedMatcherData))
            {
                return new ReferenceConverter(referencedMatcherData.Match);
            }
            if (_referenceManager.TryGetMatcherData(referenceName, matcherIndex, out var matcherData))
            {
                var context = CreateNestedContext(groupNames, referencePrefix);
                var builder = Resolve(matcherData.ModifierBuilder, context);
                var referencedBuilder = ModifierResultToReferencedBuilder(builder.Build());
                return new ReferenceConverter(referencedBuilder);
            }
            return new ReferenceConverter(null);
        }

        private static object ModifierResultToReferencedBuilder(IModifierResult result)
        {
            if (result.Entries.Count != 1)
                throw new ParseException(
                    $"Referenced matchers must have exactly one ModifierResultEntry, {result.Entries.Count} given ({result})");

            var entry = result.Entries.Single();
            if (entry.Value != null)
                throw new ParseException($"Referenced matchers may not have values ({entry})");
            if (entry.Form != null)
                throw new ParseException($"Referenced matchers may not have forms ({entry})");
            if (entry.Stat == null)
                throw new ParseException($"Referenced matchers must have stats ({entry})");
            var stat = result.StatConverter(entry.Stat);
            if (entry.Condition != null)
            {
                stat = stat.WithCondition(entry.Condition);
            }

            return stat;
        }

        private static IModifierBuilder Resolve(IModifierBuilder unresolvedBuilder, ResolveContext context)
        {
            var oldResult = unresolvedBuilder.Build();
            return new ModifierBuilder()
                .WithValues(oldResult.Entries.Select(e => e.Value?.Resolve(context)))
                .WithForms(oldResult.Entries.Select(e => e.Form?.Resolve(context)))
                .WithStats(oldResult.Entries.Select(e => e.Stat?.Resolve(context)))
                .WithConditions(oldResult.Entries.Select(e => e.Condition?.Resolve(context)))
                .WithValueConverter(v => oldResult.ValueConverter(v)?.Resolve(context))
                .WithStatConverter(s => oldResult.StatConverter(s)?.Resolve(context));
        }
    }
}