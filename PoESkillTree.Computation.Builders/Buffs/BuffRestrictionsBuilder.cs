using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Buffs
{
    internal class BuffRestrictionsBuilder : IResolvable<BuffRestrictionsBuilder>
    {
        private readonly IEnumerable<IKeywordBuilder> _restrictedToKeywords;
        private readonly IEnumerable<IKeywordBuilder> _excludedKeywords;

        public BuffRestrictionsBuilder()
        {
            _restrictedToKeywords = Enumerable.Empty<IKeywordBuilder>();
            _excludedKeywords = Enumerable.Empty<IKeywordBuilder>();
        }

        private BuffRestrictionsBuilder(
            IEnumerable<IKeywordBuilder> restrictedToKeywords, IEnumerable<IKeywordBuilder> excludedKeywords)
        {
            _restrictedToKeywords = restrictedToKeywords;
            _excludedKeywords = excludedKeywords;
        }

        public BuffRestrictionsBuilder Resolve(ResolveContext context) =>
            new BuffRestrictionsBuilder(
                _restrictedToKeywords.Select(b => b.Resolve(context)),
                _excludedKeywords.Select(b => b.Resolve(context)));

        public BuffRestrictionsBuilder With(IKeywordBuilder keyword) =>
            new BuffRestrictionsBuilder(_restrictedToKeywords.Append(keyword), _excludedKeywords);

        public BuffRestrictionsBuilder Without(IKeywordBuilder keyword) =>
            new BuffRestrictionsBuilder(_restrictedToKeywords, _excludedKeywords.Append(keyword));

        public BuffRestrictions Build() =>
            new BuffRestrictions(
                _restrictedToKeywords.Select(b => b.Build()).ToList(),
                _excludedKeywords.Select(b => b.Build()).ToList());
    }
}