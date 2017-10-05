using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            IParser<IModifierResult> CreateInnerParser(IStatMatchers statMatchers) =>
                new CachingParser<IModifierResult>(
                    new StatNormalizingParser<IModifierResult>(
                        new ParserWithResultSelector<IModifierBuilder,IModifierResult>(
                            new DummyParser(new StatMatcherRegexExpander(statMatchers)), // TODO
                            b => b?.Build())));

            var statMatchersList = CreateStatMatchers(new BuilderFactories(),
                new MatchContextsStub(), new ModifierBuilder());
            var statMatchersFactory = new StatMatchersSelector(statMatchersList);
            var innerParserCache = new Dictionary<IStatMatchers, IParser<IModifierResult>>();
            IStep<IParser<IModifierResult>, bool> initialStep =
                new MappingStep<IStatMatchers, IParser<IModifierResult>, bool>(
                    new MappingStep<ParsingStep, IStatMatchers, bool>(
                        new SpecialStep(),
                        statMatchersFactory.Get
                    ),
                    statMatchers => innerParserCache.GetOrAdd(statMatchers, CreateInnerParser)
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
         */

        // Obviously only temporary until the actually useful classes exist
        private class DummyParser : IParser<IModifierBuilder>
        {
            private readonly IEnumerable<MatcherData> _statMatchers;

            public DummyParser(IEnumerable<MatcherData> statMatchers)
            {
                _statMatchers = statMatchers;
            }

            public bool TryParse(string stat, out string remaining, out IModifierBuilder result)
            {
                var xs =
                    from m in _statMatchers
                    let match = Regex.Match(stat, m.Regex) // TODO regex should be cached
                    where match.Success
                    orderby match.Length descending
                    let replaced = stat.Substring(0, match.Index)
                                   + match.Result(m.MatchSubstitution)
                                   + stat.Substring(match.Index + match.Length)
                    select new { m.ModifierBuilder, match.Value, Result = replaced, match.Groups };

                var x = xs.FirstOrDefault();
                if (x == null)
                {
                    result = null;
                    remaining = stat;
                    return false;
                }
                result = x.ModifierBuilder;
                remaining = x.Result;
                return true;
            }
        }
    }
}
