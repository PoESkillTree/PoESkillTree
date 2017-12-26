using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing
{
    public class Parser : IParser
    {
        private readonly IReadOnlyList<IReferencedMatchers> _referencedMatchers;
        private readonly IReadOnlyList<IStatMatchers> _statMatchers;
        private readonly IReadOnlyList<StatReplacerData> _statReplacers;
        private readonly IBuilderFactories _builderFactories;

        private readonly Lazy<IParser<IReadOnlyList<Modifier>>> _parser;

        public Parser(
            IReadOnlyList<IReferencedMatchers> referencedMatchers, 
            IReadOnlyList<IStatMatchers> statMatchers,
            IReadOnlyList<StatReplacerData> statReplacers,
            IBuilderFactories builderFactories)
        {
            _referencedMatchers = referencedMatchers;
            _statMatchers = statMatchers;
            _statReplacers = statReplacers;
            _builderFactories = builderFactories;
            _parser = new Lazy<IParser<IReadOnlyList<Modifier>>>(CreateParser);
        }

        public bool TryParse(string stat, out string remaining, out IReadOnlyList<Modifier> result)
        {
            return _parser.Value.TryParse(stat, out remaining, out result);
        }

        private IParser<IReadOnlyList<Modifier>> CreateParser()
        {
            var referenceManager = new ReferenceManager(_referencedMatchers, _statMatchers);
            var regexGroupService = new RegexGroupService(_builderFactories.ValueBuilders);

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

            var statMatchersFactory = new StatMatchersSelector(_statMatchers);
            var innerParserCache = new Dictionary<IStatMatchers, IParser<IModifierResult>>();
            IStep<IParser<IModifierResult>, bool> initialStep =
                new MappingStep<IStatMatchers, IParser<IModifierResult>, bool>(
                    new MappingStep<ParsingStep, IStatMatchers, bool>(
                        new SpecialStep(),
                        statMatchersFactory.Get
                    ),
                    k => innerParserCache.GetOrAdd(k, CreateInnerParser)
                );

            return
                new CachingParser<IReadOnlyList<Modifier>>(
                    new ValidatingParser<IReadOnlyList<Modifier>>(
                        new StatNormalizingParser<IReadOnlyList<Modifier>>(
                            new ParserWithResultSelector<IReadOnlyList<IReadOnlyList<Modifier>>,
                                IReadOnlyList<Modifier>>(
                                new StatReplacingParser<IReadOnlyList<Modifier>>(
                                    new ParserWithResultSelector<IReadOnlyList<IModifierResult>,
                                        IReadOnlyList<Modifier>>(
                                        new CompositeParser<IModifierResult>(initialStep),
                                        l => l.Aggregate().Build(_builderFactories.ConditionBuilders.True)),
                                    _statReplacers
                                ),
                                ls => ls.Flatten().ToList()
                            )
                        )
                    )
                );
        }
    }
}