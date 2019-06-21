using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Parsing.Referencing;
using PoESkillTree.Computation.Parsing.StringParsers;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="ICoreParser" /> using the parsing pipeline laid out by this project.
    /// <para> Dependencies not instantiated here are the actual data (lists of <see cref="IReferencedMatchers" />,
    /// <see cref="IStatMatchers" /> and <see cref="StatReplacerData" />), contained in the <c>Computation.Data</c>
    /// project, and an implementation of the interfaces in <see cref="Common.Builders" />. These must be passed to the
    /// constructor.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <see cref="CreateParser" /> is a good overview to learn how the parts in this project interact.
    /// </remarks>
    public class CoreParser<TStep> : ICoreParser
    {
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(CoreParser<>).Assembly, $"{typeof(CoreParser<>).FullName}<{typeof(TStep).Name}>");

        private delegate StringParseResult<IReadOnlyList<Modifier>> Parser(CoreParserParameter parameter);

        private readonly IParsingData<TStep> _parsingData;
        private readonly IBuilderFactories _builderFactories;

        private readonly Lazy<Parser> _parser;

        private readonly ConcurrentDictionary<CoreParserParameter, ParseResult> _cache =
            new ConcurrentDictionary<CoreParserParameter, ParseResult>();

        public CoreParser(IParsingData<TStep> parsingData, IBuilderFactories builderFactories)
        {
            _parsingData = parsingData;
            _builderFactories = builderFactories;
            _parser = new Lazy<Parser>(CreateParser);
        }

        public ParseResult Parse(CoreParserParameter parameter)
            => _cache.GetOrAdd(parameter, ParseCacheMiss);

        private ParseResult ParseCacheMiss(CoreParserParameter parameter)
        {
            try
            {
                var (success, remaining, result) = _parser.Value(parameter);
                if (success)
                {
                    return ParseResult.Success(result);
                }
                var parseResult = ParseResult.Failure(parameter.ModifierLine, remaining);
                Log.Debug($"ParseResult.Failure({parseResult})");
                return parseResult;
            }
            catch (ParseException e)
            {
                Log.Error("ParseException while parsing " + parameter, e);
                return ParseResult.Failure(parameter.ModifierLine, parameter.ModifierLine);
            }
        }

        private Parser CreateParser()
        {
            var referenceService = new ReferenceService(_parsingData.ReferencedMatchers, _parsingData.StatMatchers);
            var regexGroupService = new RegexGroupService(_builderFactories.ValueBuilders);

            // The parsing pipeline using one IStatMatchers instance to parse a part of the stat.
            IStringParser<IIntermediateModifier> CreateInnerParser(IStatMatchers statMatchers) =>
                new CachingStringParser<IIntermediateModifier>(
                    new StatNormalizingParser<IIntermediateModifier>(
                        new ResolvingParser(
                            MatcherDataParser.Create(
                                statMatchers.Data,
                                new StatMatcherRegexExpander(
                                    referenceService, regexGroupService, statMatchers.MatchesWholeLineOnly).Expand),
                            referenceService,
                            new IntermediateModifierResolver(ModifierBuilder.Empty),
                            regexGroupService
                        )
                    )
                );

            Parallel.ForEach(_parsingData.StatMatchers, s => { _ = s.Data; });
            var innerParserCache = _parsingData.StatMatchers.ToDictionary(Funcs.Identity, CreateInnerParser);

            // The steps define the order in which the inner parsers, and by extent the IStatMatchers, are executed.
            IStringParser<IIntermediateModifier> StepToParser(TStep step)
                => innerParserCache[_parsingData.SelectStatMatcher(step)];

            // The full parsing pipeline.
            IStringParser<IReadOnlyList<IReadOnlyList<IIntermediateModifier>>> parser =
                new ValidatingParser<IReadOnlyList<IReadOnlyList<IIntermediateModifier>>>(
                    new StatNormalizingParser<IReadOnlyList<IReadOnlyList<IIntermediateModifier>>>(
                        new StatReplacingParser<IReadOnlyList<IIntermediateModifier>>(
                            new CompositeParser<IIntermediateModifier, TStep>(_parsingData.Stepper, StepToParser),
                            _parsingData.StatReplacers
                        )
                    )
                );
            return ps => AggregateAndBuild(ps, parser.Parse(ps.ModifierLine));
        }

        private static StringParseResult<IReadOnlyList<Modifier>> AggregateAndBuild(
            CoreParserParameter parameter,
            StringParseResult<IReadOnlyList<IReadOnlyList<IIntermediateModifier>>> result)
        {
            var (successfullyParsed, remaining, intermediates) = result;
            var modifiers = intermediates.SelectMany(ms => AggregateAndBuild(parameter, ms)).ToList();
            return (successfullyParsed, remaining, modifiers);
        }

        private static IReadOnlyList<Modifier> AggregateAndBuild(
            CoreParserParameter parameter, IReadOnlyList<IIntermediateModifier> intermediates)
            => intermediates
                .Aggregate()
                .Build(parameter.ModifierSource, parameter.ModifierSourceEntity);
    }
}