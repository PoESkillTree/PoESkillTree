using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel.Items;
using static PoESkillTree.Computation.Common.Builders.Values.ValueBuilderUtils;

namespace PoESkillTree.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying forms, values and stats.
    /// </summary>
    public class FormAndStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public FormAndStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder)
            : base(builderFactories, matchContexts)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IEnumerable<MatcherData> CreateCollection() =>
            new FormAndStatMatcherCollection(_modifierBuilder, ValueFactory)
            {
                // attributes
                // offense
                // - damage
                {
                    @"adds # to # ({DamageTypeMatchers}) damage",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]), Reference.AsDamageType.Damage.WithHits
                },
                {
                    @"# to # added ({DamageTypeMatchers}) damage",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]), Reference.AsDamageType.Damage.WithHits
                },
                {
                    "# to # ({DamageTypeMatchers}) damage",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]), Reference.AsDamageType.Damage.WithHits
                },
                {
                    @"adds # to # ({DamageTypeMatchers}) damage to attacks",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]),
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack)
                },
                {
                    @"adds # to # ({DamageTypeMatchers}) damage to unarmed attacks",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]),
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Attack).With(Keyword.Melee),
                    Not(MainHand.HasItem)
                },
                {
                    @"adds # to # ({DamageTypeMatchers}) damage to spells",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]),
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Spell)
                },
                {
                    @"# to # additional ({DamageTypeMatchers}) damage",
                    BaseAdd, ValueFactory.FromMinAndMax(Values[0], Values[1]), Reference.AsDamageType.Damage.WithHits
                },
                {
                    @"adds # maximum ({DamageTypeMatchers}) damage",
                    BaseAdd, Value.MaximumOnly, Reference.AsDamageType.Damage.WithHits
                },
                { "deal no ({DamageTypeMatchers}) damage", TotalOverride, 0, Reference.AsDamageType.Damage },
                {
                    @"explosion deals (base )?({DamageTypeMatchers}) damage equal to #% of the (corpse|monster)'s maximum life",
                    BaseSet, Value.AsPercentage * Life.For(Enemy).Value,
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Secondary)
                },
                {
                    @"explosion deals # to # base ({DamageTypeMatchers}) damage",
                    BaseSet, ValueFactory.FromMinAndMax(Values[0], Values[1]),
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Secondary)
                },
                {
                    "deals # to # base ({DamageTypeMatchers}) damage",
                    BaseSet, ValueFactory.FromMinAndMax(Values[0], Values[1]),
                    Reference.AsDamageType.Damage.WithSkills(DamageSource.Spell)
                },
                // - conversion and gain
                {
                    "(gain )?#% of ({DamageTypeMatchers}) damage (gained |added )?as (extra )?({DamageTypeMatchers}) damage",
                    BaseAdd, Value, References[0].AsDamageType.Damage.WithHitsAndAilments
                        .GainAs(References[1].AsDamageType.Damage.WithHitsAndAilments)
                },
                {
                    "gain #% of ({DamageTypeMatchers}) damage as extra damage of a random element",
                    BaseAdd, Value, Reference.AsDamageType.Damage.WithHitsAndAilments
                        .GainAs(RandomElement.Damage.WithHitsAndAilments)
                },
                {
                    "gain #% of wand ({DamageTypeMatchers}) damage as extra ({DamageTypeMatchers}) damage",
                    BaseAdd, Value,
                    References[0].AsDamageType.Damage.With(AttackDamageHand.MainHand)
                        .GainAs(References[1].AsDamageType.Damage.With(AttackDamageHand.MainHand))
                        .WithCondition(MainHand.Has(Tags.Wand)),
                    References[0].AsDamageType.Damage.With(AttackDamageHand.OffHand)
                        .GainAs(References[1].AsDamageType.Damage.With(AttackDamageHand.OffHand))
                        .WithCondition(OffHand.Has(Tags.Wand))
                },
                {
                    "#% of ({DamageTypeMatchers}) damage converted to ({DamageTypeMatchers}) damage",
                    BaseAdd, Value, References[0].AsDamageType.Damage.WithHitsAndAilments
                        .ConvertTo(References[1].AsDamageType.Damage.WithHitsAndAilments)
                },
                // - penetration
                {
                    "damage penetrates #% (of enemy )?({DamageTypeMatchers}) resistances?",
                    BaseAdd, Value, Reference.AsDamageType.Penetration
                },
                {
                    "damage (?<inner>with .*|dealt by .*) penetrates #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Value, Reference.AsDamageType.Penetration, "${inner}"
                },
                {
                    "penetrates? #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Value, Reference.AsDamageType.Penetration
                },
                {
                    "({KeywordMatchers}) damage penetrates #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Value, References[1].AsDamageType.Penetration.With(References[0].AsKeyword)
                },
                {
                    "({KeywordMatchers}) damage (?<inner>with .*|dealt by .*) penetrates #% ({DamageTypeMatchers}) resistances?",
                    BaseAdd, Value, References[1].AsDamageType.Penetration.With(References[1].AsKeyword), "${inner}"
                },
                // - crit
                { @"\+#% critical strike chance", BaseAdd, Value, CriticalStrike.Chance },
                {
                    "no critical strike multiplier, no damage multiplier for ailments from critical strikes",
                    TotalOverride, 0, CriticalStrike.Multiplier
                },
                { "never deal critical strikes", TotalOverride, 0, CriticalStrike.Chance },
                // - speed
                { "actions are #% slower", PercentLess, Value, Stat.ActionSpeed },
                // - projectiles
                { "fires # additional (projectiles|arrows)", BaseAdd, Value, Projectile.Count },
                { "fires an additional (projectile|arrow)", BaseAdd, 1, Projectile.Count },
                { "skills fire an additional projectile", BaseAdd, 1, Projectile.Count },
                { "supported skills fire # additional projectiles", BaseAdd, Value, Projectile.Count },
                { "pierces # additional targets", BaseAdd, Value, Projectile.PierceCount },
                { "projectiles pierce an additional target", BaseAdd, 1, Projectile.PierceCount },
                { "(projectiles|arrows) pierce # (additional )?targets", BaseAdd, Value, Projectile.PierceCount },
                {
                    "projectiles from supported skills pierce # additional targets", BaseAdd, Value,
                    Projectile.PierceCount
                },
                {
                    "projectiles pierce all nearby targets",
                    TotalOverride, double.PositiveInfinity, Projectile.PierceCount, Enemy.IsNearby
                },
                {
                    "projectiles pierce all targets",
                    TotalOverride, double.PositiveInfinity, Projectile.PierceCount
                },
                { @"chains \+# times", BaseAdd, Value, Projectile.ChainCount },
                { @"(supported )?skills chain \+# times", BaseAdd, Value, Projectile.ChainCount },
                // - other
                { "your hits can't be evaded", TotalOverride, 100, Stat.ChanceToHit },
                { "can't be evaded", TotalOverride, 100, Stat.ChanceToHit },
                // defense
                // - life, mana, defences
                { "maximum life becomes #", TotalOverride, Value, Life },
                { "removes all mana", TotalOverride, 0, Mana },
                { "converts all evasion rating to armour", TotalOverride, 100, Evasion.ConvertTo(Armour) },
                { "cannot evade enemy attacks", TotalOverride, 0, Evasion.Chance },
                { @"\+# evasion rating", BaseAdd, Value, Evasion },
                // - resistances
                { "immune to ({DamageTypeMatchers}) damage", TotalOverride, 100, Reference.AsDamageType.Resistance },
                { @"\+#% elemental resistances", BaseAdd, Value, Elemental.Resistance },
                { @"\+?#% physical damage reduction", BaseAdd, Value, Physical.Resistance },
                // - leech
                {
                    "life leech is applied to energy shield instead",
                    TotalOverride, (int) Pool.EnergyShield, Life.Leech.TargetPool
                },
                { "gain life from leech instantly", TotalOverride, 1, Life.InstantLeech },
                { "leech #% of damage as life", BaseAdd, Value, Life.Leech.Of(Damage) },
                // - block
                {
                    "#% of block chance applied to spells",
                    BaseAdd, Value.PercentOf(Block.AttackChance), Block.SpellChance
                },
                // - other
                {
                    "chaos damage does not bypass energy shield",
                    TotalOverride, 100, Chaos.DamageTakenFrom(EnergyShield).Before(Life)
                },
                {
                    "#% of chaos damage does not bypass energy shield",
                    BaseAdd, Value, Chaos.DamageTakenFrom(EnergyShield).Before(Life)
                },
                {
                    "#% of physical damage bypasses energy shield",
                    BaseSubtract, Value, Physical.DamageTakenFrom(EnergyShield).Before(Life)
                },
                {
                    "you take #% reduced extra damage from critical strikes",
                    PercentReduce, Value, CriticalStrike.ExtraDamageTaken
                },
                {
                    "you take no extra damage from critical strikes",
                    PercentLess, 100, CriticalStrike.ExtraDamageTaken
                },
                // regen and recharge 
                // (need to be FormAndStatMatcher because they also exist with flat values)
                {
                    "#%( of)? ({PoolStatMatchers}) regenerated per second",
                    BaseAdd, Value, Reference.AsPoolStat.Regen.Percent
                },
                {
                    "#% of ({PoolStatMatchers}) and ({PoolStatMatchers}) regenerated per second",
                    BaseAdd, Value, References[0].AsPoolStat.Regen.Percent, References[1].AsPoolStat.Regen.Percent
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
                { "life regeneration has no effect", PercentLess, 100, Life.Regen },
                {
                    "life regeneration is applied to energy shield instead",
                    TotalOverride, (int) Pool.EnergyShield, Life.Regen.TargetPool
                },
                // gain (need to be FormAndStatMatcher because they also exist with flat values)
                {
                    "#% of ({PoolStatMatchers}) gained",
                    BaseAdd, Value.PercentOf(Reference.AsStat), Reference.AsPoolStat.Gain
                },
                {
                    "recover #% of( their)? ({PoolStatMatchers})",
                    BaseAdd, Value.PercentOf(Reference.AsStat), Reference.AsPoolStat.Gain
                },
                {
                    "removes #% of ({PoolStatMatchers})",
                    BaseSubtract, Value.PercentOf(Reference.AsStat), Reference.AsPoolStat.Gain
                },
                { @"\+# ({PoolStatMatchers}) gained", BaseAdd, Value, Reference.AsPoolStat.Gain },
                { @"gain \+# ({PoolStatMatchers})", BaseAdd, Value, Reference.AsPoolStat.Gain },
                // charges
                {
                    "#% chance to gain a power, frenzy or endurance charge",
                    BaseAdd, Value / 3,
                    Charge.Power.ChanceToGain, Charge.Frenzy.ChanceToGain, Charge.Endurance.ChanceToGain
                },
                {
                    "(?<!chance to |when you )gain an? ({ChargeTypeMatchers})",
                    BaseAdd, 100, Reference.AsChargeType.ChanceToGain
                },
                // skills
                { "base duration is # seconds", BaseSet, Value, Stat.Duration },
                { "#% reduced duration", PercentReduce, Value, Stat.Duration },
                { "skills cost no mana", TotalOverride, 0, Mana.Cost },
                // traps, mines, totems
                { "trap lasts # seconds", BaseSet, Value, Stat.Trap.Duration },
                { "mine lasts # seconds", BaseSet, Value, Stat.Mine.Duration },
                { "totem lasts # seconds", BaseSet, Value, Stat.Totem.Duration },
                {
                    "detonating mines is instant",
                    TotalOverride, double.PositiveInfinity, Stat.CastRate, With(Skills.DetonateMines)
                },
                // minions
                { "can summon up to # golem at a time", BaseSet, Value, Golems.CombinedInstances.Maximum },
                // buffs
                {
                    "(?<!while |chance to )you have ({BuffMatchers})",
                    TotalOverride, 1, Reference.AsBuff.NotAsBuffOn(Self)
                },
                {
                    "(?<!while |chance to )gain ({BuffMatchers})",
                    TotalOverride, 1, Reference.AsBuff.On(Self)
                },
                {
                    "you can have one additional curse",
                    BaseAdd, 1, Buff.CurseLimit
                },
                {
                    "enemies can have # additional curse",
                    BaseAdd, Value, Buff.CurseLimit.For(Enemy)
                },
                { "unaffected by curses", PercentLess, 100, Buffs(targets: Self).With(Keyword.Curse).Effect },
                { "immune to curses", TotalOverride, 0, Buffs(targets: Self).With(Keyword.Curse).On },
                {
                    "monsters are hexproof",
                    TotalOverride, 0, Buffs(Self, Enemy).With(Keyword.Curse).On, Flag.IgnoreHexproof.IsSet.Not
                },
                { "grants? fortify", TotalOverride, 1, Buff.Fortify.On(Self) },
                { "gain elemental conflux", TotalOverride, 1, Buff.Conflux.Elemental.On(Self) },
                { "({BuffMatchers}) lasts # seconds", BaseSet, Value, Reference.AsBuff.Duration },
                {
                    "supported auras do not affect you",
                    TotalOverride, 0, Skills.ModifierSourceSkill.Buff.EffectOn(Self)
                },
                // flags
                // ailments
                { "causes bleeding", TotalOverride, 100, Ailment.Bleed.Chance },
                { "bleed is applied", TotalOverride, 100, Ailment.Bleed.Chance },
                { "always poison", TotalOverride, 100, Ailment.Poison.Chance },
                { "always ({AilmentMatchers}) enemies", TotalOverride, 100, Reference.AsAilment.Chance },
                { "cannot cause bleeding", TotalOverride, 0, Ailment.Bleed.Chance },
                { "cannot ignite", TotalOverride, 0, Ailment.Ignite.Chance },
                { "cannot apply shock", TotalOverride, 0, Ailment.Shock.Chance },
                { "cannot inflict elemental ailments", TotalOverride, 0, Ailment.Elemental.Select(s => s.Chance) },
                {
                    "(you )?can afflict an additional ignite on an enemy",
                    BaseAdd, 1, Ailment.Ignite.InstancesOn(Enemy).Maximum
                },
                { "(you are )?immune to ({AilmentMatchers})", TotalOverride, 100, Reference.AsAilment.Avoidance },
                { "cannot be ({AilmentMatchers})", TotalOverride, 100, Reference.AsAilment.Avoidance },
                {
                    "cannot be ({AilmentMatchers}) or ({AilmentMatchers})",
                    TotalOverride, 100, References[0].AsAilment.Avoidance, References[1].AsAilment.Avoidance
                },
                {
                    "(immune to|cannot be affected by) elemental ailments",
                    TotalOverride, 100, Ailment.Elemental.Select(a => a.Avoidance)
                },
                {
                    "poison you inflict with critical strikes deals #% more damage",
                    PercentMore, Value, CriticalStrike.Multiplier.With(Ailment.Poison)
                },
                // stun
                { "(you )?cannot be stunned", TotalOverride, 100, Effect.Stun.Avoidance },
                { "additional #% chance to be stunned", BaseAdd, Value, Effect.Stun.Chance.For(Entity.OpponentOfSelf) },
                // item quantity/quality
                // range and area of effect
                // other
                { "knocks back enemies", TotalOverride, 100, Effect.Knockback.Chance },
                { "knocks enemies back", TotalOverride, 100, Effect.Knockback.Chance },
            };
    }
}