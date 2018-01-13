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

        protected override IEnumerable<ReferencedMatcherData> CreateCollection() =>
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