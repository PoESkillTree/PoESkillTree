using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
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
            var regexGroupService = new RegexGroupService(builderFactories.ValueBuilders);

            IParser<IModifierResult> CreateInnerParser(IStatMatchers statMatchers) =>
                new CachingParser<IModifierResult>(
                    new StatNormalizingParser<IModifierResult>(
                        new ResolvingParser(
                            new MatcherDataParser(
                                new StatMatcherRegexExpander(statMatchers, referenceManager, regexGroupService)),
                            referenceManager,
                            new ModifierResultResolver(new ModifierBuilder()),
                            regexGroupService
                        )
                    )
                );

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
    }
}
