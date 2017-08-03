using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;
using static PoESkillTree.Computation.Providers.Values.ValueProviderUtils;

namespace PoESkillTree.Computation.Data
{
    public class ValueConversionMatchers : UsesMatchContext, IStatMatchers
    {
        // These apply to the main value of the modifier (or multiple e.g. for "Adds # to # ..."),
        // not to other values like in "for # seconds".

        public ValueConversionMatchers(IProviderFactories providerFactories,
            IMatchContextFactory matchContextFactory) 
            : base(providerFactories, matchContextFactory)
        {
            Matchers = CreateCollection().ToList();
        }

        public IReadOnlyList<MatcherData> Matchers { get; }

        private ValueConversionMatcherCollection CreateCollection() =>
            new ValueConversionMatcherCollection
            {
                // action
                { "for each enemy you've killed recently", Kill.CountRecently },
                {
                    "per enemy killed by you or your totems recently",
                    Kill.CountRecently + Kill.By(Entity.Totem).CountRecently
                },
                { "for each hit you've blocked recently", Block.CountRecently },
                { "for each corpse consumed recently", Action.ConsumeCorpse.CountRecently },
                // equipment
                { "for each type of golem you have summoned", Golems.Count(s => s.HasInstance) },
                {
                    "for each magic item you have equipped",
                    Equipment.Count(e => e.Has(FrameType.Magic))
                },
                // stats
                {
                    "per # ({StatMatchers})",
                    PerStat(stat: Group.AsStat, divideBy: Value)
                },
                {
                    "per # ({StatMatchers}) ceiled",
                    PerStatCeiled(stat: Group.AsStat, divideBy: Value)
                },
                { "per ({StatMatchers})", PerStat(stat: Group.AsStat) },
                { "per Level", PerStat(Self.Level) },
                // buffs
                {
                    "per buff on you",
                    Buffs(target: Self).ExceptFrom(Skill.BloodRage, Skill.MoltenShell).Count()
                },
                { "per curse on you", Buffs(target: Self).With(Keyword.Curse).Count() },
                {
                    "for each curse on that enemy,",
                    Buffs(target: Enemy).With(Keyword.Curse).Count()
                },
                // ailments
                { "for each poison on the enemy", Ailment.Poison.InstancesOn(Enemy).Value },
                { "per poison on enemy", Ailment.Poison.InstancesOn(Enemy).Value },
                // skills
                { "for each zombie you own", Skill.RaiseZombie.Instances.Value },
                // traps, mines, totems
                { "for each trap", Traps.CombinedInstances.Value },
                { "for each mine", Mines.CombinedInstances.Value },
                {
                    "for each trap and mine you have",
                    Traps.CombinedInstances.Value + Mines.CombinedInstances.Value
                },
                { "per totem", Totems.CombinedInstances.Value },
            };
    }
}