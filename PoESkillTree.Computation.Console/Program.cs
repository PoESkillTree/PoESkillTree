using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builderFactories = new BuilderFactories();
            var statMatchersList = CreateStatMatchers(builderFactories,
                new MatchContextsStub(), new ModifierBuilder());
            var referencedMatchersList = CreateReferencedMatchers(builderFactories);
            var referenceManager = new ReferenceManager(referencedMatchersList, statMatchersList);

            IParser<IModifierResult> CreateInnerParser(IStatMatchers statMatchers) =>
                new CachingParser<IModifierResult>(
                    new StatNormalizingParser<IModifierResult>(
                        new ParserWithResultSelector<IModifierBuilder,IModifierResult>(
                            new DummyParser( // TODO
                                new StatMatcherRegexExpander(statMatchers, referenceManager), referenceManager),
                            b => b?.Build())));

            var statMatchersFactory = new StatMatchersSelector(statMatchersList);
            var innerParserCache = new Dictionary<IStatMatchers, IParser<IModifierResult>>();
            IStep<IParser<IModifierResult>, bool> initialStep =
                new MappingStep<IStatMatchers, IParser<IModifierResult>, bool>(
                    new MappingStep<ParsingStep, IStatMatchers, bool>(
                        new SpecialStep(),
                        statMatchersFactory.Get
                    ),
                    k => innerParserCache.GetOrAdd(k, CreateInnerParser)
                );

            IParser<IReadOnlyList<Modifier>> parser =
                new CachingParser<IReadOnlyList<Modifier>>(
                    new ValidatingParser<IReadOnlyList<Modifier>>(
                        new StatNormalizingParser<IReadOnlyList<Modifier>>(
                            new ParserWithResultSelector<IReadOnlyList<IReadOnlyList<Modifier>>,
                                IReadOnlyList<Modifier>>(
                                new StatReplacingParser<IReadOnlyList<Modifier>>(
                                    new ParserWithResultSelector<IReadOnlyList<IModifierResult>,
                                        IReadOnlyList<Modifier>>(
                                        new CompositeParser<IModifierResult>(initialStep),
                                        l => l.Aggregate().Build()),
                                    new StatReplacers().Replacers
                                ),
                                ls => ls.Flatten().ToList()
                            )
                        )
                    )
                );

            ReferenceValidator.Validate(referencedMatchersList, statMatchersList);

            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != "")
            {
                if (!parser.TryParse(statLine, out var remaining, out var result))
                {
                    System.Console.WriteLine($"Not recognized: '{remaining}' could not be parsed.");
                }
                System.Console.WriteLine(result == null ? "null" : string.Join("\n", result));
                System.Console.Write("> ");
            }
        }

        private static IReadOnlyList<IStatMatchers> CreateStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts,
            IModifierBuilder modifierBuilder) => new IStatMatchers[]
        {
            new SpecialMatchers(builderFactories, matchContexts, modifierBuilder),
            new StatManipulatorMatchers(builderFactories, matchContexts, modifierBuilder),
            new ValueConversionMatchers(builderFactories, matchContexts, modifierBuilder),
            new FormAndStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new FormMatchers(builderFactories, matchContexts, modifierBuilder),
            new GeneralStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new DamageStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new PoolStatMatchers(builderFactories, matchContexts, modifierBuilder),
            new ConditionMatchers(builderFactories, matchContexts, modifierBuilder),
        };

        private static IReadOnlyList<IReferencedMatchers> CreateReferencedMatchers(
            IBuilderFactories builderFactories) => new IReferencedMatchers[]
        {
            new ActionMatchers(builderFactories.ActionBuilders),
            new AilmentMatchers(builderFactories.EffectBuilders.Ailment), 
            new ChargeTypeMatchers(builderFactories.ChargeTypeBuilders), 
            new DamageTypeMatchers(builderFactories.DamageTypeBuilders), 
            new FlagMatchers(builderFactories.StatBuilders.Flag),
            new ItemSlotMatchers(), 
            new KeywordMatchers(builderFactories.KeywordBuilders), 
            new SkillMatchers(builderFactories.SkillBuilders),
        };

        /* Proper implementation missing: (replacing DummyParser in InnerParser() function)
         * - leaf parsers (what DummyParser does)
         * - some parser doing the match context resolving after leaf parser
         *   (what Resolve() does)
         */

        private class DummyParser : IParser<IModifierBuilder>
        {
            private readonly IEnumerable<MatcherData> _statMatchers;
            private readonly IReferenceToMatcherDataResolver _referenceManager;

            public DummyParser(IEnumerable<MatcherData> statMatchers, IReferenceToMatcherDataResolver referenceManager)
            {
                _statMatchers = statMatchers;
                _referenceManager = referenceManager;
            }

            private static Regex CreateRegex(string regex)
            {
                return new Regex(regex,
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            }

            public bool TryParse(string stat, out string remaining, out IModifierBuilder result)
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
                    select new { m.ModifierBuilder, match.Value, Result = replaced, Groups = SelectGroups(regex, match.Groups) };

                var x = xs.FirstOrDefault();
                if (x == null)
                {
                    result = null;
                    remaining = stat;
                    return false;
                }
                result = Resolve(x.ModifierBuilder, x.Groups, _referenceManager);
                remaining = x.Result;
                return true;
            }

            private static IReadOnlyDictionary<string, string> SelectGroups(
                Regex regex,
                GroupCollection groups)
            {
                return regex.GetGroupNames()
                    .Where(gn => !string.IsNullOrEmpty(groups[gn].Value))
                    .ToDictionary(gn => gn, gn => groups[gn].Value);
            }
        }

        private static IModifierBuilder Resolve(
            IModifierBuilder builder,
            IReadOnlyDictionary<string, string> groups,
            IReferenceToMatcherDataResolver referenceManager)
        {
            var values =
                from pair in groups
                let groupName = pair.Key
                where groupName.StartsWith("value")
                select BuilderFactory.CreateValue(pair.Value);
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
                select Resolve(referenceManager, referenceName, matcherIndex, referencePrefix, groupNames);
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(references.ToList());

            var context = new ResolveContext(valueContext, referenceContext);
            return Resolve(builder, context);
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

        private static IReferenceConverter Resolve(
            IReferenceToMatcherDataResolver referenceManager, 
            string referenceName, 
            int matcherIndex, 
            string referencePrefix,
            IReadOnlyList<string> groups)
        {
            if (referenceManager.TryGetReferencedMatcherData(referenceName, matcherIndex, out var referencedMatcherData))
            {
                return new ReferenceConverter(referencedMatcherData.Match);
            }
            if (referenceManager.TryGetMatcherData(referenceName, matcherIndex, out var matcherData))
            {
                var valueContext = new ResolvedMatchContext<IValueBuilder>(new IValueBuilder[0]);

                var nestedReferences =
                    from groupName in groups
                    where groupName.StartsWith(referencePrefix)
                    let suffix = groupName.Substring(referencePrefix.Length)
                    let parts = suffix.Split('_')
                    where parts.Length == 3
                    let nestedReferenceName = parts[1]
                    let nestedMatcherIndex = int.Parse(parts[2])
                    let nestedReferencePrefix = referencePrefix + parts[0] + "_"
                    select Resolve(referenceManager, nestedReferenceName, nestedMatcherIndex, nestedReferencePrefix, groups);
                var referenceContext = new ResolvedMatchContext<IReferenceConverter>(nestedReferences.ToList());

                var context = new ResolveContext(valueContext, referenceContext);
                var builder = Resolve(matcherData.ModifierBuilder, context);
                // TODO ModifierBuilder may contain a condition that also needs to be taken into account
                // TODO validate that it only has one entry with stat and optionally condition and no converters?
                return new ReferenceConverter(builder.Build().Entries.First().Stat);
            }
            return new ReferenceConverter(null);
        }
    }
}
