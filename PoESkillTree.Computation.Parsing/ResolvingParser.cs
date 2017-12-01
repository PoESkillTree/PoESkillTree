using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Parsing
{
    // TODO tests
    public class ResolvingParser : IParser<IModifierBuilder>
    {
        private readonly IParser<MatcherDataParseResult> _innerParser;
        private readonly IReferenceToMatcherDataResolver _referenceManager;

        private readonly IModifierBuilderResolver _modifierBuilderResolver;

        private readonly IRegexGroupParser _regexGroupParser;

        private IReadOnlyDictionary<string, string> _groups;

        public ResolvingParser(
            IParser<MatcherDataParseResult> innerParser,
            IReferenceToMatcherDataResolver referenceManager, 
            IModifierBuilderResolver modifierBuilderResolver,
            IRegexGroupParser regexGroupParser)
        {
            _innerParser = innerParser;
            _referenceManager = referenceManager;
            _modifierBuilderResolver = modifierBuilderResolver;
            _regexGroupParser = regexGroupParser;
        }

        public bool TryParse(string stat, out string remaining, out IModifierBuilder result)
        {
            if (!_innerParser.TryParse(stat, out remaining, out var innerResult))
            {
                result = innerResult?.ModifierBuilder;
                return false;
            }
            _groups = innerResult.Groups;
            var context = CreateContext("");
            result = _modifierBuilderResolver.Resolve(innerResult.ModifierBuilder, context);
            return true;
        }

        private ResolveContext CreateContext(string groupPrefix)
        {
            var values = _regexGroupParser
                .ParseValues(_groups)
                .ToList();
            var valueContext = new ResolvedMatchContext<IValueBuilder>(values);

            var references = _regexGroupParser
                .ParseReferences(_groups, groupPrefix)
                .Select(t => ResolveNested(t.referenceName, t.matcherIndex, t.groupPrefix))
                .ToList();
            var referenceContext = new ResolvedMatchContext<IReferenceConverter>(references);

            return new ResolveContext(valueContext, referenceContext);
        }

        private IReferenceConverter ResolveNested(
            string referenceName,
            int matcherIndex,
            string groupPrefix)
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
                    _modifierBuilderResolver.ResolveToReferencedBuilder(matcherData.ModifierBuilder, context);
                return new ReferenceConverter(referencedBuilder);
            }
            return new ReferenceConverter(null);
        }
    }
}