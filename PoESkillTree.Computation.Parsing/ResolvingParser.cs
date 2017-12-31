using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Wraps an <c>IParser&lt;MatcherDataParseResult&gt;</c> and resolves
    /// <see cref="MatcherDataParseResult.Modifier"/> using references and values specified by 
    /// <see cref="MatcherDataParseResult.RegexGroups"/> before outputing the resolved <see cref="IIntermediateModifier"/>.
    /// </summary>
    /// <remarks>
    /// Values can simply be parsed from the regex group's captured substring. References are resolved to 
    /// <see cref="Data.ReferencedMatcherData"/> or <see cref="Data.MatcherData"/> using a 
    /// <see cref="IReferenceToMatcherDataResolver"/>. Referenced <see cref="Data.MatcherData"/> may itself contain
    /// references, requiring recursion.
    /// </remarks>
    public class ResolvingParser : IParser<IIntermediateModifier>
    {
        private readonly IParser<MatcherDataParseResult> _innerParser;
        private readonly IReferenceToMatcherDataResolver _referenceManager;
        private readonly IIntermediateModifierResolver _modifierResolver;
        private readonly IRegexGroupParser _regexGroupParser;

        private IReadOnlyDictionary<string, string> _groups;

        public ResolvingParser(
            IParser<MatcherDataParseResult> innerParser,
            IReferenceToMatcherDataResolver referenceManager,
            IIntermediateModifierResolver modifierResolver,
            IRegexGroupParser regexGroupParser)
        {
            _innerParser = innerParser;
            _referenceManager = referenceManager;
            _modifierResolver = modifierResolver;
            _regexGroupParser = regexGroupParser;
        }

        public ParseResult<IIntermediateModifier> Parse(string stat)
        {
            var (successfullyParsed, remaining, innerResult) = _innerParser.Parse(stat);
            IIntermediateModifier result;

            if (successfullyParsed)
            {
                _groups = innerResult.RegexGroups;
                var context = CreateContext("");
                result = _modifierResolver.Resolve(innerResult.Modifier, context);
            }
            else
            {
                result = innerResult?.Modifier;
            }

            return (successfullyParsed, remaining, result);
        }

        private ResolveContext CreateContext(string groupPrefix)
        {
            IReadOnlyList<IValueBuilder> values = _regexGroupParser
                .ParseValues(_groups, groupPrefix)
                .ToList();
            var valueContext = new ResolvedMatchContext<IValueBuilder>(values);

            IReadOnlyList<IReferenceConverter> references = _regexGroupParser
                .ParseReferences(_groups.Keys, groupPrefix)
                .Select(t => ResolveNested(t.referenceName, t.matcherIndex, t.groupPrefix))
                .ToList();
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(references);

            return new ResolveContext(valueContext, referenceContext);
        }

        private IReferenceConverter ResolveNested(string referenceName, int matcherIndex, string groupPrefix)
        {
            if (_referenceManager.TryGetReferencedMatcherData(referenceName, matcherIndex,
                out var referencedMatcherData))
            {
                return new ReferenceConverter(referencedMatcherData.Match);
            }

            if (_referenceManager.TryGetMatcherData(referenceName, matcherIndex, out var matcherData))
            {
                var context = CreateContext(groupPrefix);
                var referencedBuilder =
                    _modifierResolver.ResolveToReferencedBuilder(matcherData.Modifier, context);
                return new ReferenceConverter(referencedBuilder);
            }

            throw new ParseException(
                $"Unknown reference, name={referenceName}, matcherIndex={matcherIndex}, groupPrefix={groupPrefix}");
        }
    }
}