using System.Collections.Generic;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;

using DamageStat = PoESkillTree.Computation.Providers.Stats.IDamageStatProvider;

namespace PoESkillTree.Computation.Data
{
    public class StatManipulatorMatchers : UsesMatchContext, IStatMatchers
    {
        public StatManipulatorMatchers(IProviderFactories providerFactories, 
            IMatchContextFactory matchContextFactory) 
            : base(providerFactories, matchContextFactory)
        {
            StatMatchers = CreateCollection();
        }

        public IEnumerable<object> StatMatchers { get; }

        private StatManipulatorMatcherCollection CreateCollection() =>
            new StatManipulatorMatcherCollection
            {
                { "you and nearby allies( deal| have)?", s => s.AsAura(Self, Ally) },
                {
                    "auras you cast grant (.*) to you and allies",
                    s => s.AddTo(Skills[Keyword.Aura]), "$1"
                },
                {
                    "consecrated ground you create grant (.*) to you and allies",
                    s => s.AddTo(Ground.Consecrated), "$1"
                },
                {
                    "every # seconds, gain (.*) for # seconds",
                    s => Buff.Rotation(Values.First).Step(Values.Last, s.AsBuff), "$1"
                },
                { "nearby enemies (have|deal)", s => s.AsAura(Enemy) },
                { "nearby enemies take", (DamageStat s) => s.Taken.AsAura(Enemy) },
                { "enemies near your totems (have|deal)", s => Entity.Totem.Stat(s.AsAura(Enemy)) },
                {
                    "enemies near your totems take",
                    (DamageStat s) => Entity.Totem.Stat(s.Taken.AsAura(Enemy))
                },
                // Keep whole mod line, take is part of the condition matcher
                { "enemies.* take", (DamageStat s) => s.Taken, "$0" },
                { "(chance to .*) for # seconds", s => s.ForXSeconds(Value).ChanceOn(Self), "$1" },
                { "for # seconds", s => s.ForXSeconds(Value).On(Self) },
            };
    }
}