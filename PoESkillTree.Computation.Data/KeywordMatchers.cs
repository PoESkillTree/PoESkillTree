using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Data;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IReferencedMatchers"/> implementation for <see cref="IKeywordBuilder"/>s.
    /// </summary>
    public class KeywordMatchers : ReferencedMatchersBase<IKeywordBuilder>
    {
        private IKeywordBuilders Keyword { get; }

        public KeywordMatchers(IKeywordBuilders keywordBuilders)
        {
            Keyword = keywordBuilders;
        }

        protected override IReadOnlyList<ReferencedMatcherData> CreateCollection() =>
            new ReferencedMatcherCollection<IKeywordBuilder>
            {
                { "melee", Keyword.Melee },
                { "projectiles?", Keyword.Projectile },
                { "golems?", Keyword.Golem },
                { "traps?", Keyword.Trap },
                { "mines?", Keyword.Mine },
                { "totems?", Keyword.Totem },
                { "curses?", Keyword.Curse },
                { "auras?", Keyword.Aura },
                { "area", Keyword.AreaOfEffect },
                { "warcry", Keyword.Warcry },
                { "herald", Keyword.Herald },
                { "brand", Keyword.Brand },
                { "movement", Keyword.Movement },
                { "banner", Keyword.Banner },
                { "channelling", Keyword.From(GameModel.Skills.Keyword.Channelling) },
                { "guard", Keyword.From(GameModel.Skills.Keyword.Guard) },
            };
    }
}