using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Skills;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Data
{
    public class KeywordMatchers : ReferencedMatchersBase<IKeywordProvider>
    {
        private IKeywordProviderFactory Keyword { get; }

        public KeywordMatchers(IKeywordProviderFactory keywordProviderFactory)
        {
            Keyword = keywordProviderFactory;
        }

        protected override IEnumerable<ReferencedMatcherData<IKeywordProvider>>
            CreateCollection() =>
            new ReferencedMatcherCollection<IKeywordProvider>
            {
                { "melee", Keyword.Melee },
                { "attacks?", Keyword.Attack },
                { "projectiles?", Keyword.Projectile },
                { "golems?", Keyword.Golem },
                { "traps?", Keyword.Trap },
                { "mines?", Keyword.Mine },
                { "totems?", Keyword.Totem },
                { "curses?", Keyword.Curse },
                { "auras?", Keyword.Aura },
                { "area", Keyword.AreaOfEffect },
                { "spells?", Keyword.Spell },
                { "warcry", Keyword.Warcry },
            };
    }
}