using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers;
using PoESkillTree.Computation.Providers.Matching;
using PoESkillTree.Computation.Providers.Stats;
using static PoESkillTree.Computation.Providers.Values.ValueProviderUtils;

namespace PoESkillTree.Computation.Data
{
    public class FormAndStatMatchers : UsesMatchContext, IStatMatchers
    {
        public FormAndStatMatchers(IProviderFactories providerFactories,
            IMatchContextFactory matchContextFactory)
            : base(providerFactories, matchContextFactory)
        {
            Matchers = CreateCollection().ToList();
        }

        public IReadOnlyList<MatcherData> Matchers { get; }

        private FormAndStatMatcherCollection CreateCollection() => new FormAndStatMatcherCollection
        {
            // attributes
            // offense
            // - damage
            {
                @"adds # to # ({DamageTypeMatchers}) damage",
                (MinBaseAdd, MaximumAdd), Group.AsDamageType.Damage
            },
            {
                @"# to # additional ({DamageTypeMatchers}) damage",
                (MinBaseAdd, MaximumAdd), Group.AsDamageType.Damage
            },
            {
                @"adds # maximum ({DamageTypeMatchers}) damage",
                MaxBaseAdd, Group.AsDamageType.Damage
            },
            { "deal no ({DamageTypeMatchers}) damage", Zero, Group.AsDamageType.Damage },
            // - penetration
            {
                "damage penetrates #% ({DamageTypeMatchers}) resistances?",
                BaseAdd, Group.AsDamageType.Penetration
            },
            {
                "damage (with .*) penetrates #% ({DamageTypeMatchers}) resistances?",
                BaseAdd, Group.AsDamageType.Penetration, "$1"
            },
            {
                "penetrate #% ({DamageTypeMatchers}) resistances?",
                BaseAdd, Group.AsDamageType.Penetration, "$1"
            },
            // - crit
            { @"\+#% critical strike chance", BaseAdd, CriticalStrike.Chance },
            { "no critical strike multiplier", Zero, CriticalStrike.Multiplier },
            {
                "no damage multiplier for ailments from critical strikes",
                Zero, CriticalStrike.AilmentMultiplier
            },
            { "never deal critical strikes", Zero, CriticalStrike.Chance },
            // - speed
            // - projectiles
            { "skills fire an additional projectile", BaseAdd, Projectile.Count, 1 },
            { "pierces # additional targets", BaseAdd, Projectile.PierceCount },
            { "projectiles pierce an additional target", BaseAdd, Projectile.PierceCount, 1 },
            { "projectiles pierce # targets", BaseAdd, Projectile.PierceCount },
            {
                "projectiles pierce all nearby targets",
                TotalOverride, Projectile.PierceCount, double.PositiveInfinity, Enemy.IsNearby
            },
            { @"skills chain \+# times", BaseAdd, Projectile.ChainCount },
            // - other
            { "your hits can't be evaded", Zero, Enemy.Stat(Evasion.Chance) },
            // defense
            // - life, mana, defences
            { "maximum life becomes #", TotalOverride, Life },
            { "removes all mana", Zero, Mana },
            {
                "converts all evasion rating to armour",
                TotalOverride, Evasion.ConvertTo(Armour), 100
            },
            { "cannot evade enemy attacks", Zero, Evasion.Chance },
            // - resistances
            {
                "immune to ({DamageTypeMatchers}) damage",
                TotalOverride, Group.AsDamageType.Resistance, 100
            },
            // - leech
            {
                "life leech is applied to energy shield instead", SetFlag,
                Life.Leech.AppliesTo(EnergyShield)
            },
            { "gain life from leech instantly", SetFlag, Life.InstantLeech },
            { "leech #% of damage as life", BaseAdd, Life.Leech.Of(Damage) },
            // - block
            // - other
            {
                "chaos damage does not bypass energy shield",
                Always, Chaos.Damage.TakenFrom(EnergyShield).Before(Life)
            },
            {
                "#% of chaos damage does not bypass energy shield",
                BaseAdd, Chaos.Damage.TakenFrom(EnergyShield).Before(Life),
                Chaos.Damage.TakenFrom(EnergyShield).Before(Mana)
            },
            {
                "#% of physical damage bypasses energy shield",
                BaseSubtract, Physical.Damage.TakenFrom(EnergyShield).Before(Life)
            },
            {
                "you take #% reduced extra damage from critical strikes",
                PercentReduce, CriticalStrike.ExtraDamageTaken
            },
            // regen and recharge 
            // (need to be FormAndStatMatcher because they also exist with flat values)
            {
                "#% of ({PoolStatMatchers}) regenerated per second",
                BaseAdd, Group.As<IPoolStatProvider>().Regen.Percent
            },
            {
                "#% of ({PoolStatMatchers}) and ({PoolStatMatchers}) regenerated per second",
                BaseAdd,
                Groups[0].As<IPoolStatProvider>().Regen.Percent,
                Groups[1].As<IPoolStatProvider>().Regen.Percent
            },
            {
                "regenerate #%( of)?( their)? ({PoolStatMatchers}) per second",
                BaseAdd, Group.As<IPoolStatProvider>().Regen.Percent
            },
            {
                "# ({PoolStatMatchers}) regenerated per second", BaseAdd,
                Group.As<IPoolStatProvider>().Regen
            },
            {
                "#% faster start of energy shield recharge", PercentIncrease,
                EnergyShield.Recharge.Start
            },
            { "life regeneration has no effect", PercentLess, Life.Regen, 100 },
            {
                "life regeneration is applied to energy shield instead", SetFlag,
                Life.Regen.AppliesTo(EnergyShield)
            },
            // gain (need to be FormAndStatMatcher because they also exist with flat values)
            {
                "#% of ({PoolStatMatchers}) gained",
                BaseAdd, Group.As<IPoolStatProvider>().Gain, PercentOf(Group.AsStat)
            },
            {
                "recover #% of( their)? ({PoolStatMatchers})",
                BaseAdd, Group.As<IPoolStatProvider>().Gain, PercentOf(Group.AsStat)
            },
            {
                "removes #% of ({PoolStatMatchers})",
                BaseSubtract, Group.As<IPoolStatProvider>().Gain, PercentOf(Group.AsStat)
            },
            { @"\+# ({PoolStatMatchers}) gained", BaseAdd, Group.As<IPoolStatProvider>().Gain },
            // charges
            // skills
            // traps, mines, totems
            {
                "detonating mines is instant",
                TotalOverride, Skill.DetonateMines.Speed, double.PositiveInfinity
            },
            // minions
            // buffs
            {
                "you can have one additional curse",
                BaseAdd, Buffs(target: Self).With(Keyword.Curse).CombinedLimit, 1
            },
            {
                "enemies can have # additional curse",
                BaseAdd, Buffs(target: Enemy).With(Keyword.Curse).CombinedLimit
            },
            { "grants fortify", SetFlag, Buff.Fortify.On(Self) },
            { "you have fortify", SetFlag, Buff.Fortify.On(Self) },
            {
                @"curse enemies with level # ({SkillMatchers})",
                SetFlag, Buff.Curse(skill: Group.AsSkill, level: Value).On(Enemy)
            },
            { "gain elemental conflux", SetFlag, Buff.Conflux.Elemental.On(Self) },
            // flags
            {
                "(?<!while )(you have|gain) ({FlagMatchers})", SetFlag,
                Group.As<IFlagStatProvider>()
            },
            // ailments
            { "causes bleeding", Always, Ailment.Bleed.Chance },
            { "always poison", Always, Ailment.Poison.Chance },
            {
                "(you )?can afflict an additional ignite on an enemy",
                BaseAdd, Ailment.Ignite.InstancesOn(Enemy).Maximum, 1
            },
            { "you are immune to ({AilmentMatchers})", Always, Group.AsAilment.Avoidance },
            { "cannot be ({AilmentMatchers})", Always, Group.AsAilment.Avoidance },
            {
                "(immune to|cannot be affected by) elemental ailments",
                Always, Ailment.Elemental.Select(a => a.Avoidance)
            },
            // stun
            { "(you )?cannot be stunned", Always, Effect.Stun.Avoidance },
            { "your damaging hits always stun enemies", Always, Effect.Stun.ChanceOn(Enemy) },
            // item quantity/quality
            // range and area of effect
            // other
            { "knocks back enemies", Always, Effect.Knockback.ChanceOn(Enemy) },
        };
    }
}
