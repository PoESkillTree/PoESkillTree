using System.Collections.Generic;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var initialStep = new MappingStep<ParsingStep, IParser<string>, bool>(
                new SpecialStep(), s => new DumpParser(s));
            IParser<IReadOnlyList<string>> parser =
                new CachingParser<IReadOnlyList<string>>(
                    new ValidatingParser<IReadOnlyList<string>>(
                        new StatNormalizingParser<IReadOnlyList<string>>(
                            new StatReplacingParser<string>(
                                new CompositeParser<string, string>(
                                    initialStep, 
                                    l => string.Join("\n  ", l)),
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

        /* Parser hierarchy:
         Stat s from console
         -> CachingParser<IReadOnlyList<IMatch>>
         -> ValidatingParser<IReadOnlyList<IMatch>>
         -> StatNormalizingParser<IReadOnlyList<IMatch>>
         -> StatReplacingParser<IMatch>
         -> CompositeParser<IMatch>
            - IStep<IParser<IMatchBuilder>>:
              - new MappingStep<ParsingStep, IParser<IMatchBuilder>>(new SpecialStep(), ?)
              - Func<ParsingStep, IStatMatcher>: ConventionResolver<TEnum, TOut>
                - maps from ParsingStep to the IStatMatcher implementations
              - Func<IStatMatcher, IParser<IMatchBuilder>> = x => new ?Parser(x)
                - the ?Parser is where most of the logic happens
              - Func<IParser<IMatchBuilder>, IParser<IMatchBuilder>> that consists of several 
                chained constructor calls
                - CachingParser, StatNormalizingParser/some postprocessing (merge multiple spaces)
         */
        /* Algorithm missing:
         * - IMatchBuilder aggregator (Func<IReadOnlyList<IMatchBuilder>, IMatch>)
         * - leaf parsers (some class implementing IParser<IMatchBuilder> and using an IStatMatcher)
         */

        // Obviously only temporary until the actually useful classes exist
        private class DumpParser : IParser<string>
        {
            private readonly ParsingStep _parsingStep;

            public DumpParser(ParsingStep parsingStep)
            {
                _parsingStep = parsingStep;
            }

            public bool TryParse(string stat, out string remaining, out string result)
            {
                result = _parsingStep + ": " + stat;
                remaining = string.Empty;
                return true;
            }
        }
    }
}
