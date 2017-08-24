using System.Collections.Generic;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IParser<string> InnerParser(IStatMatchers statMatchers) =>
                new CachingParser<string>(
                    new StatNormalizingParser<string>(
                        new DummyParser(statMatchers))); // TODO

            var statMatchersFactory = new StatMatchersSelector(CreateStatMatchers());
            IStep<IParser<string>, bool> initialStep =
                new MappingStep<IStatMatchers, IParser<string>, bool>(
                    new MappingStep<ParsingStep, IStatMatchers, bool>(
                        new SpecialStep(),
                        statMatchersFactory.Get
                    ),
                    InnerParser
                );

            IParser<IReadOnlyList<string>> parser =
                new CachingParser<IReadOnlyList<string>>(
                    new ValidatingParser<IReadOnlyList<string>>(
                        new StatNormalizingParser<IReadOnlyList<string>>(
                            new StatReplacingParser<string>(
                                new CompositeParser<string, string>(
                                    initialStep,
                                    l => string.Join("\n  ", l) // TODO
                                ),
                                new StatReplacers().Replacers
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

        private static IReadOnlyList<IStatMatchers> CreateStatMatchers() => new IStatMatchers[]
        {
            new SpecialMatchers(null, null, null),
            new StatManipulatorMatchers(null, null, null), 
            new ValueConversionMatchers(null, null, null),
            new FormAndStatMatchers(null, null, null),
            new FormMatchers(null, null),
            new GeneralStatMatchers(null, null, null),
            new DamageStatMatchers(null, null, null), 
            new PoolStatMatchers(null, null, null),
            new ConditionMatchers(null, null, null),
        };

        /* Algorithm missing:
         * - IModifierBuilder aggregator (Func<IReadOnlyList<IModifierBuilder>, IModifier>)
         *   (replacing second argument of CompositeParser constructor)
         * - leaf parsers (some class implementing IParser<IModifierBuilder> and using an IStatMatcher)
         *   (replacing DummyParser in InnerParser() function)
         * - implementations of IBuilderFactories, IMatchContexts, IModifierBuilder
         *   (replacing nulls in CreateStatMatchers())
         */

        // Obviously only temporary until the actually useful classes exist
        private class DummyParser : IParser<string>
        {
            private readonly IStatMatchers _statMatchers;

            public DummyParser(IStatMatchers statMatchers)
            {
                _statMatchers = statMatchers;
            }

            public bool TryParse(string stat, out string remaining, out string result)
            {
                result = _statMatchers + ": " + stat;
                remaining = string.Empty;
                return true;
            }
        }
    }
}
