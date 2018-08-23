using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Buffs
{
    internal class BuffRestrictions
    {
        private readonly IReadOnlyList<Keyword> _restrictedToKeywords;
        private readonly IReadOnlyList<Keyword> _excludedKeywords;

        public BuffRestrictions(IReadOnlyList<Keyword> restrictedToKeywords, IReadOnlyList<Keyword> excludedKeywords)
        {
            _restrictedToKeywords = restrictedToKeywords;
            _excludedKeywords = excludedKeywords;
        }

        public bool AllowsBuff(BuffBuilderWithKeywords buff) =>
            _restrictedToKeywords.Intersect(buff.Keywords).Count() == _restrictedToKeywords.Count &&
            _excludedKeywords.Intersect(buff.Keywords).IsEmpty();
    }
}