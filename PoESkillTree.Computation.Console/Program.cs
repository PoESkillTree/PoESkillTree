using System.Collections.Generic;
using PoESkillTree.Common;
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
                                new CompositeParser<string, string>(
                                    new SessionFactory(),
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
            - ?ParsingSession<IMatchBuilder>
            - Func<IReadOnlyList<IMatchBuilder>, IMatch>
            - ?Parser<IMatchBuilder>
         */
        /* Algorithm missing:
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

        private class SessionFactory : IFactory<IParsingSession<string>>
        {
            public IParsingSession<string> Create()
            {
                return new Session();
            }
        }

        private class Session : IParsingSession<string>
        {
            public Session()
            {
                CurrentParser = new DumpParser();
            }

            public bool Completed { get; private set; }
            public bool Successful { get; private set; }
            public IParser<string> CurrentParser { get; }

            public void ParseSuccessful()
            {
                Completed = true;
                Successful = true;
            }

            public void ParseFailed()
            {
                Completed = true;
                Successful = false;
            }
        }
    }
}
