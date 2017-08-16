using System.Collections.Generic;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IParser<IReadOnlyList<string>> parser =
                new CachingParser<IReadOnlyList<string>>(
                    new ValidatingParser<IReadOnlyList<string>>(
                        new StatNormalizingParser<IReadOnlyList<string>>(
                            new StatReplacingParser<string>(
                                new DumpParser(),
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
            - Start new parsing session: strategy.CreateSession()
            - remaining = s
            - builders = []
            - Until session.Completed:
              - IParser<IMatchBuilder> next = session.CurrentParser
              - If next.TryParse(remaining, out builder, out remaining):
                - builders.Add(builder)
                - session.ParseSuccessful()
              - Else:
                - session.ParseFailed()
            - Return session.Successful and output (Aggregate(builders), remaining)
         */
        /* Algorithm missing:
         * - parsing strategy
         * - parsing session
         * - IMatchBuilder aggregator
         * - leaf parsers 
         *   (they also have a postprocessor that merges multiple spaces in remaining into one
         *    /run StatNormalizingParser before them)
         */

        // Obviously only temporary until the actually useful classes exist
        private class DumpParser : IParser<string>
        {
            public bool TryParse(string stat, out string remaining, out string result)
            {
                result = stat;
                remaining = string.Empty;
                return true;
            }
        }
    }
}
