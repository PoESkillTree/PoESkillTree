using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Data;
using PoESkillTree.Computation.Parsing.ModifierBuilding;
using static PoESkillTree.Computation.Parsing.Builders.Values.ValueBuilderUtils;

namespace PoESkillTree.Computation.Data
{
    public class FormAndStatMatchers : UsesMatchContext, IStatMatchers
    {
        private readonly IModifierBuilder _modifierBuilder;

        public FormAndStatMatchers(IBuilderFactories builderFactories,
            IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        public bool MatchesWholeLineOnly => false;

        public IEnumerator<MatcherData> GetEnumerator() => new FormAndStatMatcherCollection(
            _modifierBuilder, ValueFactory)
        {
            // attributes
            // offense
            // - damage
            {
                @"adds # to # ({DamageTypeMatchers}) damage",
                (MinBaseAdd, MaximumAdd), (Values[0], Values[1]), Reference.AsDamageType.Damage
            },
            {
                @"# to # additional ({DamageTypeMatchers}) damage",
                (MinBaseAdd, MaximumAdd), (Values[0], Values[1]), Reference.AsDamageType.Damage
            },
            {
                @"adds # maximum ({DamageTypeMatchers}) damage",
                MaxBaseAdd, Value, Reference.AsDamageType.Damage
            },
            { "deal no ({DamageTypeMatchers}) damage", TotalOverride, 0, Reference.AsDamageType.Damage },
            // - penetration
            {
                "damage penetrates #% (of enemy )?({DamageTypeMatchers}) resistances?",
                BaseAdd, Value, Reference.AsDamageType.Penetration
            },
            {
                "damage (?<inner>with .*) penetrates #% ({DamageTypeMatchers}) resistances?",
                BaseAdd, Value, Reference.AsDamageType.Penetration, "${inner}"
            },
            {
                "penetrate #% ({DamageTypeMatchers}) resistances?",
                BaseAdd, Value, Reference.AsDamageType.Penetration
            },
            // - crit
            { @"\+#% critical strike chance", BaseAdd, Value, CriticalStrike.Chance },
            { "no critical strike multiplier", TotalOverride, 0, CriticalStrike.Multiplier },
            {
                "no damage multiplier for ailments from critical strikes",
                TotalOverride, 0, CriticalStrike.AilmentMultiplier
            },
            { "never deal critical strikes", TotalOverride, 0, CriticalStrike.Chance },
            // - speed
            // - projectiles
            { "skills fire an additional projectile", BaseAdd, 1, Projectile.Count },
            { "pierces # additional targets", BaseAdd, Value, Projectile.PierceCount },
            { "projectiles pierce an additional target", BaseAdd, 1, Projectile.PierceCount },
            { "projectiles pierce # (additional )?targets", BaseAdd, Value, Projectile.PierceCount },
            {
                "projectiles pierce all nearby targets",
                TotalOverride, double.PositiveInfinity, Projectile.PierceCount, Enemy.IsNearby
            },
            { @"skills chain \+# times", BaseAdd, Value, Projectile.ChainCount },
            // - other
            { "your hits can't be evaded", TotalOverride, 0, Enemy.Stat(Evasion.Chance) },
            // defense
            // - life, mana, defences
            { "maximum life becomes #", TotalOverride, Value, Life },
            { "removes all mana", TotalOverride, 0, Mana },
            {
                "converts all evasion rating to armour",
                TotalOverride, 100, Evasion.ConvertTo(Armour)
            },
            { "cannot evade enemy attacks", TotalOverride, 0, Evasion.Chance },
            // - resistances
            {
                "immune to ({DamageTypeMatchers}) damage",
                TotalOverride, 100, Reference.AsDamageType.Resistance
            },
            { @"\+#% elemental resistances", BaseAdd, Value, Elemental.Resistance },
            { @"\+?#% physical damage reduction", BaseAdd, Value, Physical.Resistance },
            // - leech
            {
                "life leech is applied to energy shield instead", TotalOverride, 1,
                Life.Leech.AppliesTo(EnergyShield)
            },
            { "gain life from leech instantly", TotalOverride, 1, Life.InstantLeech },
            { "leech #% of damage as life", BaseAdd, Value, Life.Leech.Of(Damage) },
            // - block
            // - other
            {
                "chaos damage does not bypass energy shield",
                TotalOverride, 100, Chaos.Damage.TakenFrom(EnergyShield).Before(Life)
            },
            {
                "#% of chaos damage does not bypass energy shield",
                BaseAdd, Value, Chaos.Damage.TakenFrom(EnergyShield).Before(Life),
                Chaos.Damage.TakenFrom(EnergyShield).Before(Mana)
            },
            {
                "#% of physical damage bypasses energy shield",
                BaseSubtract, Value, Physical.Damage.TakenFrom(EnergyShield).Before(Life)
            },
            {
                "you take #% reduced extra damage from critical strikes",
                PercentReduce, Value, CriticalStrike.ExtraDamageTaken
            },
            // regen and recharge 
            // (need to be FormAndStatMatcher because they also exist with flat values)
            {
                "#% of ({PoolStatMatchers}) regenerated per second",
                BaseAdd, Value, Reference.AsPoolStat.Regen.Percent
            },
            {
                "#% of ({PoolStatMatchers}) and ({PoolStatMatchers}) regenerated per second",
                BaseAdd, Value,
                References[0].AsPoolStat.Regen.Percent,
                References[1].AsPoolStat.Regen.Percent
            },
            {
                "regenerate #%( of)?( their| your)? ({PoolStatMatchers}) per second",
                BaseAdd, Value, Reference.AsPoolStat.Regen.Percent
            },
            {
                "# ({PoolStatMatchers}) regenerated per second", BaseAdd, Value,
                Reference.AsPoolStat.Regen
            },
            {
                "#% faster start of energy shield recharge", PercentIncrease, Value,
                EnergyShield.Recharge.Start
            },
            { "life regeneration has no effect", TotalOverride, 0, Life.Regen },
            {
                "life regeneration is applied to energy shield instead", TotalOverride, 1,
                Life.Regen.AppliesTo(EnergyShield)
            },
            // gain (need to be FormAndStatMatcher because they also exist with flat values)
            {
                "#% of ({PoolStatMatchers}) gained",
                BaseAdd, Value, Reference.AsPoolStat.Gain, PercentOf(Reference.AsStat)
            },
            {
                "recover #% of( their)? ({PoolStatMatchers})",
                BaseAdd, Value, Reference.AsPoolStat.Gain, PercentOf(Reference.AsStat)
            },
            {
                "removes #% of ({PoolStatMatchers})",
                BaseSubtract, Value, Reference.AsPoolStat.Gain, PercentOf(Reference.AsStat)
            },
            { @"\+# ({PoolStatMatchers}) gained", BaseAdd, Value, Reference.AsPoolStat.Gain },
            // charges
            // skills
            // traps, mines, totems
            {
                "detonating mines is instant",
                TotalOverride, double.PositiveInfinity, Skill.DetonateMines.Speed
            },
            // minions
            // buffs
            {
                "you can have one additional curse",
                BaseAdd, 1, Buffs(target: Self).With(Keyword.Curse).CombinedLimit
            },
            {
                "enemies can have # additional curse",
                BaseAdd, Value, Buffs(target: Enemy).With(Keyword.Curse).CombinedLimit
            },
            { "grants fortify", TotalOverride, 1, Buff.Fortify.On(Self) },
            { "(?<!while )you have fortify", TotalOverride, 1, Buff.Fortify.On(Self) },
            {
                @"curse enemies with level # ({SkillMatchers})",
                TotalOverride, 1, Buff.Curse(skill: Reference.AsSkill, level: Value).On(Enemy)
            },
            { "gain elemental conflux", TotalOverride, 1, Buff.Conflux.Elemental.On(Self) },
            // flags
            {
                "(?<!while |chance to )(you have|gain) ({FlagMatchers})", TotalOverride, 1,
                Reference.AsFlagStat
            },
            // ailments
            { "causes bleeding", TotalOverride, 100, Ailment.Bleed.Chance },
            { "always poison", TotalOverride, 100, Ailment.Poison.Chance },
            {
                "(you )?can afflict an additional ignite on an enemy",
                BaseAdd, 1, Ailment.Ignite.InstancesOn(Enemy).Maximum
            },
            { "you are immune to ({AilmentMatchers})", TotalOverride, 100, Reference.AsAilment.Avoidance },
            { "cannot be ({AilmentMatchers})", TotalOverride, 100, Reference.AsAilment.Avoidance },
            {
                "(immune to|cannot be affected by) elemental ailments",
                TotalOverride, 100, Ailment.Elemental.Select(a => a.Avoidance)
            },
            // stun
            { "(you )?cannot be stunned", TotalOverride, 100, Effect.Stun.Avoidance },
            { "your damaging hits always stun enemies", TotalOverride, 100, Effect.Stun.ChanceOn(Enemy) },
            // item quantity/quality
            // range and area of effect
            // other
            { "knocks back enemies", TotalOverride, 100, Effect.Knockback.ChanceOn(Enemy) },
        }.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
