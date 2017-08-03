using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Skills;

namespace PoESkillTree.Computation.Data
{
    public class KeywordMatchers : IReferencedMatchers<IKeywordProvider>
    {
        private IKeywordProviderFactory Keyword { get; }

        public KeywordMatchers(IKeywordProviderFactory keywordProviderFactory)
        {
            Keyword = keywordProviderFactory;

            Matchers = CreateCollection().ToList();
        }

        public string ReferenceName { get; } = nameof(KeywordMatchers);

        public IReadOnlyList<ReferencedMatcherData<IKeywordProvider>> Matchers { get; }

        private MatcherCollection<IKeywordProvider> CreateCollection() =>
            new MatcherCollection<IKeywordProvider>
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