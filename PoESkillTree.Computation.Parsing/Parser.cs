using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Parsing.Referencing;
using PoESkillTree.Computation.Parsing.StringParsers;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="IParser" /> using the parsing pipeline laid out by this project.
    /// <para> Dependencies not instantiated here are the actual data (lists of <see cref="IReferencedMatchers" />,
    /// <see cref="IStatMatchers" /> and <see cref="StatReplacerData" />), contained in the <c>Computation.Data</c>
    /// project, and an implementation of the interfaces in <see cref="Common.Builders" />. These must be passed to the
    /// constructor.
    /// </para>
    /// <para> This should be the only <see cref="IStringParser{TResult}" /> implementation that is relevant outside of
    /// this project (excluding their own tests, obviously).
    /// </para>
    /// </summary>
    /// <remarks>
    /// <see cref="CreateParser" /> is a good overview to learn how the parts in this project interact.
    /// </remarks>
    public class Parser<TStep> : IParser
    {
        private readonly IParsingData<TStep> _parsingData;
        private readonly IBuilderFactories _builderFactories;

        private readonly Lazy<IStringParser<IReadOnlyList<Modifier>>> _parser;

        private readonly Dictionary<CacheKey, ParseResult> _cache = new Dictionary<CacheKey, ParseResult>();

        private CacheKey _currentKey;

        public Parser(IParsingData<TStep> parsingData, IBuilderFactories builderFactories)
        {
            _parsingData = parsingData;
            _builderFactories = builderFactories;
            _parser = new Lazy<IStringParser<IReadOnlyList<Modifier>>>(CreateParser);
        }

        public ParseResult Parse(string stat, ModifierSource modifierSource, Entity modifierSourceEntity)
        {
            var key = new CacheKey(stat, modifierSource, modifierSourceEntity);
            return _cache.GetOrAdd(key, Parse);
        }

        private ParseResult Parse(CacheKey key)
        {
            _currentKey = key;
            var (success, remaining, result) = _parser.Value.Parse(key.Stat);
            return new ParseResult(success, remaining, result);
        }

        private IStringParser<IReadOnlyList<Modifier>> CreateParser()
        {
            var referenceService = new ReferenceService(_parsingData.ReferencedMatchers, _parsingData.StatMatchers);
            var regexGroupService = new RegexGroupService(_builderFactories.ValueBuilders);

            // The parsing pipeline using one IStatMatchers instance to parse a part of the stat.
            IStringParser<IIntermediateModifier> CreateInnerParser(IStatMatchers statMatchers) =>
                new CachingParser<IIntermediateModifier>(
                    new StatNormalizingParser<IIntermediateModifier>(
                        new ResolvingParser(
                            new MatcherDataParser(
                                new StatMatcherRegexExpander(statMatchers, referenceService, regexGroupService)),
                            referenceService,
                            new IntermediateModifierResolver(new ModifierBuilder()),
                            regexGroupService
                        )
                    )
                );

            var innerParserCache = new Dictionary<IStatMatchers, IStringParser<IIntermediateModifier>>();
            // The steps define the order in which the inner parsers, and by extent the IStatMatchers, are executed.
            IStringParser<IIntermediateModifier> StepToParser(TStep step) =>
                innerParserCache.GetOrAdd(_parsingData.SelectStatMatcher(step), CreateInnerParser);

            // The full parsing pipeline.
            return
                new ValidatingParser<IReadOnlyList<Modifier>>(
                    new StatNormalizingParser<IReadOnlyList<Modifier>>(
                        new ResultMappingParser<IReadOnlyList<IReadOnlyList<Modifier>>, IReadOnlyList<Modifier>>(
                            new StatReplacingParser<IReadOnlyList<Modifier>>(
                                new ResultMappingParser<IReadOnlyList<IIntermediateModifier>,
                                    IReadOnlyList<Modifier>>(
                                    new CompositeParser<IIntermediateModifier, TStep>(_parsingData.Stepper,
                                        StepToParser),
                                    AggregateAndBuild),
                                _parsingData.StatReplacers
                            ),
                            ls => ls.Flatten().ToList()
                        )
                    )
                );
        }

        private IReadOnlyList<Modifier> AggregateAndBuild(IReadOnlyList<IIntermediateModifier> intermediates) =>
            intermediates
                .Aggregate()
                .Build(_currentKey.ModifierSource, _currentKey.ModifierSourcEntity);

        private struct CacheKey : IEquatable<CacheKey>
        {
            public CacheKey(string stat, ModifierSource modifierSource, Entity modifierSourcEntity)
            {
                Stat = stat;
                ModifierSource = modifierSource;
                ModifierSourcEntity = modifierSourcEntity;
            }

            public string Stat { get; }
            public ModifierSource ModifierSource { get; }
            public Entity ModifierSourcEntity { get; }

            public override bool Equals(object obj) =>
                obj is CacheKey other && Equals(other);

            public bool Equals(CacheKey other) =>
                Stat == other.Stat && ModifierSource == other.ModifierSource &&
                ModifierSourcEntity == other.ModifierSourcEntity;

            public override int GetHashCode() => (Stat, ModifierSource, ModifierSourcEntity).GetHashCode();
        }
    }
}