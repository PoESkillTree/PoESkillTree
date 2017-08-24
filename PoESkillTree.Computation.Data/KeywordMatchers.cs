using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data
{
    public class KeywordMatchers : ReferencedMatchersBase<IKeywordBuilder>
    {
        private IKeywordBuilders Keyword { get; }

        public KeywordMatchers(IKeywordBuilders keywordBuilders)
        {
            Keyword = keywordBuilders;
        }

        protected override IEnumerable<ReferencedMatcherData<IKeywordBuilder>>
            CreateCollection() =>
            new ReferencedMatcherCollection<IKeywordBuilder>
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