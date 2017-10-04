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
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IParser<IModifierResult> InnerParser(IStatMatchers statMatchers) =>
                new CachingParser<IModifierResult>(
                    new StatNormalizingParser<IModifierResult>(
                        new DummyParser(statMatchers))); // TODO

            var statMatchersList = CreateStatMatchers(new BuilderFactories(),
                new MatchContextsStub(), new ModifierBuilder());
            var statMatchersFactory = new StatMatchersSelector(statMatchersList);
            IStep<IParser<IModifierResult>, bool> initialStep =
                new MappingStep<IStatMatchers, IParser<IModifierResult>, bool>(
                    new MappingStep<ParsingStep, IStatMatchers, bool>(
                        new SpecialStep(),
                        statMatchersFactory.Get
                    ),
                    InnerParser
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

        /* Algorithm missing: (replacing DummyParser in InnerParser() function)
         * - leaf parsers (some class implementing IParser<IModifierBuilder> and using an IStatMatcher)
         * - some parser doing the match context resolving after leaf parser
         *   (probably, depending on how the nested matcher regexes are implemented)
         * - parser calling IModifierBuilder.Build() after leaf parser
         */

        // Obviously only temporary until the actually useful classes exist
        private class DummyParser : IParser<IModifierResult>
        {
            public DummyParser(IStatMatchers statMatchers)
            {
            }

            public bool TryParse(string stat, out string remaining, out IModifierResult result)
            {
                result = SimpleModifierResult.Empty;
                remaining = string.Empty;
                return true;
            }
        }
    }
}
