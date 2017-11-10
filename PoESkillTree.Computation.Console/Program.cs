using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
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
                        new ParserWithResultSelector<IModifierBuilder, IModifierResult>(
                            new DummyParser( // TODO
                                new MatcherDataParser(
                                    new StatMatcherRegexExpander(statMatchers, referenceManager)),
                                referenceManager),
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
                try
                {
                    if (!parser.TryParse(statLine, out var remaining, out var result))
                    {
                        System.Console.WriteLine($"Not recognized: '{remaining}' could not be parsed.");
                    }
                    System.Console.WriteLine(result == null ? "null" : string.Join("\n", result));
                }
                catch (ParseException e)
                {
                    System.Console.WriteLine("Parsing failed: " + e.Message);
                }
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
            new ItemSlotMatchers(new ItemSlotBuildersStub()), 
            new KeywordMatchers(builderFactories.KeywordBuilders), 
            new SkillMatchers(builderFactories.SkillBuilders),
        };

        /* Proper implementation missing: (replacing DummyParser in InnerParser() function)
         * - some parser doing the match context resolving after leaf parser
         *   (what Resolve() does)
         */

        private class DummyParser : IParser<IModifierBuilder>
        {
            private readonly IParser<MatcherDataParser.Result> _innerParser;
            private readonly IReferenceToMatcherDataResolver _referenceManager;

            public DummyParser(IParser<MatcherDataParser.Result> innerParser, IReferenceToMatcherDataResolver referenceManager)
            {
                _innerParser = innerParser;
                _referenceManager = referenceManager;
            }

            public bool TryParse(string stat, out string remaining, out IModifierBuilder result)
            {
                if (!_innerParser.TryParse(stat, out remaining, out var innerResult))
                {
                    result = innerResult?.ModifierBuilder;
                    return false;
                }
                result = Resolve(innerResult.ModifierBuilder, innerResult.Groups, _referenceManager);
                return true;
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
                var modifierResult = builder.Build();
                if (modifierResult.Entries.Count != 1)
                    throw new ParseException(
                        $"Referenced matchers must have exactly one ModifierResultEntry, {modifierResult.Entries.Count} given ({modifierResult})");

                var entry = modifierResult.Entries.Single();
                if (entry.Value != null)
                    throw new ParseException($"Referenced matchers may not have values ({entry})");
                if (entry.Form != null)
                    throw new ParseException($"Referenced matchers may not have forms ({entry})");
                if (entry.Stat == null)
                    throw new ParseException($"Referenced matchers must have stats ({entry})");
                var stat = modifierResult.StatConverter(entry.Stat);
                if (entry.Condition != null)
                {
                    stat = AddCondition(stat, entry.Condition);
                }
                return new ReferenceConverter(stat);
            }
            return new ReferenceConverter(null);
        }

        private static IStatBuilder AddCondition(IStatBuilder stat, IConditionBuilder condition)
        {
            // TODO not pretty, but conditions in stats will have to be properly implemented anyway
            //      (IStatBuilder WithCondition(IConditionBuilder) as new method in IStatBuilder or something)
            var stringRepresentation = $"{stat} ({condition})";
            IStatBuilder Resolve(IStatBuilder s, ResolveContext _) => s;
            switch (stat)
            {
                case IFlagStatBuilder _:
                    return new FlagStatBuilderStub(stringRepresentation, Resolve);
                case IPoolStatBuilder _:
                    return new PoolStatBuilderStub(stringRepresentation, Resolve);
                case IDamageStatBuilder _:
                    return new DamageStatBuilderStub(stringRepresentation, Resolve);
                default:
                    return new StatBuilderStub(stringRepresentation, Resolve);
            }
        }
    }
}
