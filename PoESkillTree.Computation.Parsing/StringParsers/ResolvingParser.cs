using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Wraps an <c>IStringParser&lt;MatcherDataParseResult&gt;</c> and resolves
    /// <see cref="MatcherDataParseResult.Modifier"/> using references and values specified by 
    /// <see cref="MatcherDataParseResult.RegexGroups"/> before outputting the resolved <see cref="IIntermediateModifier"/>.
    /// </summary>
    /// <remarks>
    /// Values can simply be parsed from the regex group's captured substring. References are resolved to 
    /// <see cref="Common.Data.ReferencedMatcherData"/> or <see cref="Common.Data.MatcherData"/> using a 
    /// <see cref="IReferenceToMatcherDataResolver"/>. Referenced <see cref="Common.Data.MatcherData"/> may itself
    /// contain references, requiring recursion.
    /// </remarks>
    public class ResolvingParser : IStringParser<IIntermediateModifier>
    {
        private readonly IStringParser<MatcherDataParseResult> _innerParser;
        private readonly IReferenceToMatcherDataResolver _referenceManager;
        private readonly IIntermediateModifierResolver _modifierResolver;
        private readonly IRegexGroupParser _regexGroupParser;

        public ResolvingParser(
            IStringParser<MatcherDataParseResult> innerParser,
            IReferenceToMatcherDataResolver referenceManager,
            IIntermediateModifierResolver modifierResolver,
            IRegexGroupParser regexGroupParser)
        {
            _innerParser = innerParser;
            _referenceManager = referenceManager;
            _modifierResolver = modifierResolver;
            _regexGroupParser = regexGroupParser;
        }

        public StringParseResult<IIntermediateModifier> Parse(CoreParserParameter parameter)
        {
            var (successfullyParsed, remaining, innerResult) = _innerParser.Parse(parameter);
            IIntermediateModifier result;

            if (successfullyParsed)
            {
                var groups = innerResult.RegexGroups;
                var context = CreateContext(groups, "");
                result = _modifierResolver.Resolve(innerResult.Modifier, context);
            }
            else
            {
                result = innerResult?.Modifier;
            }

            return (successfullyParsed, remaining, result);
        }

        private ResolveContext CreateContext(IReadOnlyDictionary<string, string> groups, string groupPrefix)
        {
            var values = _regexGroupParser.ParseValues(groups, groupPrefix);
            var valueContext = new ResolvedMatchContext<IValueBuilder>(values);

            var parsedReferences = _regexGroupParser.ParseReferences(groups.Keys, groupPrefix);
            var references = new List<IReferenceConverter>(parsedReferences.Count);
            foreach (var (referenceName, matcherIndex, nestedGroupPrefix) in parsedReferences)
            {
                references.Add(ResolveNested(groups, referenceName, matcherIndex, nestedGroupPrefix));
            }
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(references);

            return new ResolveContext(valueContext, referenceContext);
        }

        private IReferenceConverter ResolveNested(
            IReadOnlyDictionary<string, string> groups, string referenceName, int matcherIndex, string groupPrefix)
        {
            if (_referenceManager.TryGetReferencedMatcherData(referenceName, matcherIndex,
                out var referencedMatcherData))
            {
                return new ResolvedReferenceConverter(referencedMatcherData.Match);
            }

            if (_referenceManager.TryGetMatcherData(referenceName, matcherIndex, out var matcherData))
            {
                var context = CreateContext(groups, groupPrefix);
                var referencedBuilder =
                    _modifierResolver.ResolveToReferencedBuilder(matcherData.Modifier, context);
                return new ResolvedReferenceConverter(referencedBuilder);
            }

            throw new ParseException(
                $"Unknown reference, name={referenceName}, matcherIndex={matcherIndex}, groupPrefix={groupPrefix}");
        }
    }
}