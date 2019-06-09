using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class DataDrivenMechanics : UsesConditionBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public DataDrivenMechanics(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(
                () => CollectionToList(CreateCollection()));
        }

        private IMetaStatBuilders MetaStats => BuilderFactories.MetaStatBuilders;

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private DataDrivenMechanicCollection CreateCollection()
            => new DataDrivenMechanicCollection(_modifierBuilder, BuilderFactories)
            {
                // skill hit damage
                // - DPS
                {
                    TotalOverride, MetaStats.SkillDpsWithHits,
                    MetaStats.AverageHitDamage.Value *
                    ValueFactory.If(Stat.HitRate.IsSet).Then(Stat.HitRate.Value)
                        .Else(MetaStats.CastRate.Value * MetaStats.SkillNumberOfHitsPerCast.Value)
                },
                // - average damage
                {
                    TotalOverride, MetaStats.AverageHitDamage,
                    CombineSource(MetaStats.AverageDamage.WithHits, CombineHandsForHitDamage)
                },
                // - average damage per source
                {
                    TotalOverride, MetaStats.AverageDamage.WithHits.With(AttackDamageHand.MainHand),
                    MetaStats.AverageDamagePerHit.With(AttackDamageHand.MainHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.AverageDamage.WithHits.With(AttackDamageHand.OffHand),
                    MetaStats.AverageDamagePerHit.With(AttackDamageHand.OffHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.AverageDamage.WithHits.With(DamageSource.Spell),
                    MetaStats.AverageDamagePerHit.With(DamageSource.Spell).Value
                },
                {
                    TotalOverride, MetaStats.AverageDamage.WithHits.With(DamageSource.Secondary),
                    MetaStats.AverageDamagePerHit.With(DamageSource.Secondary).Value
                },
                // - average damage of a successful hit per source
                {
                    TotalOverride, MetaStats.AverageDamagePerHit,
                    MetaStats.DamageWithNonCrits().WithHits,
                    MetaStats.DamageWithCrits().WithHits,
                    MetaStats.EffectiveCritChance,
                    (nonCritDamage, critDamage, critChance)
                        => nonCritDamage.Value.Average * (1 - critChance.Value) +
                           critDamage.Value.Average * critChance.Value
                },
                // - crit/non-crit damage per source and type
                {
                    TotalOverride, dt => MetaStats.DamageWithNonCrits(dt).WithHits,
                    dt => MetaStats.Damage(dt).WithHits,
                    dt => MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).WithHits,
                    dt => MetaStats.Damage(dt).WithHits.ChanceToDouble,
                    (_, damage, mult, chanceToDouble)
                        => damage.Value * mult.Value * (1 + chanceToDouble.Value.AsPercentage)
                },
                {
                    TotalOverride, dt => MetaStats.DamageWithCrits(dt).WithHits,
                    dt => MetaStats.Damage(dt).WithHits,
                    dt => MetaStats.EffectiveDamageMultiplierWithCrits(dt).WithHits,
                    dt => MetaStats.Damage(dt).WithHits.ChanceToDouble,
                    (_, damage, mult, chanceToDouble)
                        => damage.Value * mult.Value * (1 + chanceToDouble.Value.AsPercentage)
                },
                // - effective crit/non-crit damage multiplier per source and type
                {
                    BaseSet, dt => MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).WithHits,
                    dt => MetaStats.EnemyResistanceAgainstNonCrits(dt),
                    dt => DamageTaken(dt).WithHits.For(Enemy),
                    dt => DamageMultiplier(dt).WithHits,
                    (_, resistance, damageTaken, damageMulti)
                        => DamageTakenMultiplier(resistance, damageTaken) * damageMulti.Value.AsPercentage
                },
                {
                    BaseSet, dt => MetaStats.EffectiveDamageMultiplierWithCrits(dt).WithHits,
                    dt => MetaStats.EnemyResistanceAgainstCrits(dt),
                    dt => DamageTaken(dt).WithHits.For(Enemy),
                    dt => DamageMultiplier(dt).WithHits,
                    _ => CriticalStrike.Multiplier.WithHits,
                    (_, resistance, damageTaken, damageMulti, critMulti)
                        => DamageTakenMultiplier(resistance, damageTaken) * damageMulti.Value.AsPercentage
                                                                          * critMulti.Value.AsPercentage
                },
                // - enemy resistance against crit/non-crit hits per source and type
                {
                    TotalOverride, dt => MetaStats.EnemyResistanceAgainstNonCrits(dt),
                    dt => DamageTypeBuilders.From(dt).IgnoreResistanceWithNonCrits,
                    dt => DamageTypeBuilders.From(dt).PenetrationWithNonCrits,
                    (dt, ignoreResistance, penetration)
                        => ValueFactory.If(ignoreResistance.IsSet).Then(0)
                            .Else(DamageTypeBuilders.From(dt).Resistance.For(Enemy).Value - penetration.Value)
                },
                {
                    TotalOverride, dt => MetaStats.EnemyResistanceAgainstCrits(dt),
                    dt => DamageTypeBuilders.From(dt).IgnoreResistanceWithCrits,
                    dt => DamageTypeBuilders.From(dt).PenetrationWithCrits,
                    (dt, ignoreResistance, penetration)
                        => ValueFactory.If(ignoreResistance.Value.Eq(1)).Then(0)
                            .Else(DamageTypeBuilders.From(dt).Resistance.For(Enemy).Value - penetration.Value)
                },

                // skill damage over time
                // - DPS = average damage = non-crit damage
                {
                    TotalOverride, MetaStats.SkillDpsWithDoTs,
                    MetaStats.AverageDamage.WithSkills(DamageSource.OverTime).Value
                },
                {
                    TotalOverride, MetaStats.AverageDamage.WithSkills(DamageSource.OverTime),
                    MetaStats.DamageWithNonCrits().WithSkills(DamageSource.OverTime).Value
                },
                // - damage per type
                {
                    TotalOverride, dt => MetaStats.DamageWithNonCrits(dt).WithSkills(DamageSource.OverTime),
                    dt => MetaStats.Damage(dt).WithSkills(DamageSource.OverTime).Value *
                          MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).WithSkills(DamageSource.OverTime).Value
                },
                // - effective damage multiplier per type
                {
                    BaseSet,
                    dt => MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).WithSkills(DamageSource.OverTime),
                    dt => EnemyDamageTakenMultiplier(dt, DamageTaken(dt).WithSkills(DamageSource.OverTime))
                          * DamageMultiplier(dt).WithSkills(DamageSource.OverTime).Value.AsPercentage
                },

                // ailment damage (modifiers for EffectiveDamageMultiplierWith[Non]Crits() and Damage() are added below
                // this collection initializer)
                // - DPS
                {
                    TotalOverride, MetaStats.AilmentDps,
                    ailment => MetaStats.AverageAilmentDamage(ailment).Value *
                               MetaStats.AilmentEffectiveInstances(ailment).Value *
                               Ailment.From(ailment).TickRateModifier.Value
                },
                // - average damage
                {
                    TotalOverride, ailment => MetaStats.AverageAilmentDamage(ailment),
                    ailment => CombineSource(MetaStats.AverageDamage.With(Ailment.From(ailment)),
                        CombineHandsForAverageAilmentDamage(ailment))
                },
                // - lifetime damage of one instance
                {
                    TotalOverride, MetaStats.AilmentInstanceLifetimeDamage,
                    ailment => MetaStats.AverageAilmentDamage(ailment).Value * Ailment.From(ailment).Duration.Value
                },
                // - average damage per source
                {
                    TotalOverride, ailment => MetaStats.AverageDamage.With(Ailment.From(ailment)),
                    ailment => MetaStats.DamageWithNonCrits().With(Ailment.From(ailment)),
                    ailment => MetaStats.DamageWithCrits().With(Ailment.From(ailment)),
                    _ => MetaStats.EffectiveCritChance,
                    ailment => Ailment.From(ailment).Chance,
                    ailment => MetaStats.AilmentChanceWithCrits(ailment),
                    AverageAilmentDamageFromCritAndNonCrit
                },
                // - crit/non-crit damage per source and type
                {
                    TotalOverride, (a, dt) => MetaStats.DamageWithNonCrits(dt).With(Ailment.From(a)),
                    (a, dt) => MetaStats.Damage(dt).With(Ailment.From(a)),
                    (a, dt) => MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).With(Ailment.From(a)),
                    (damage, mult) => damage.Value * mult.Value
                },
                {
                    TotalOverride, (a, dt) => MetaStats.DamageWithCrits(dt).With(Ailment.From(a)),
                    (a, dt) => MetaStats.Damage(dt).With(Ailment.From(a)),
                    (a, dt) => MetaStats.EffectiveDamageMultiplierWithCrits(dt).With(Ailment.From(a)),
                    (damage, mult) => damage.Value * mult.Value
                },

                // speed
                {
                    // Attack is set through ItemPropertyParser if the slot is not empty
                    BaseSet, Stat.CastRate.With(AttackDamageHand.MainHand),
                    Stat.BaseCastTime.With(AttackDamageHand.MainHand).Value.Invert,
                    Not(MainHand.HasItem)
                },
                {
                    BaseSet, Stat.CastRate.With(DamageSource.Spell),
                    Stat.BaseCastTime.With(DamageSource.Spell).Value.Invert
                },
                {
                    BaseSet, Stat.CastRate.With(DamageSource.Secondary),
                    Stat.BaseCastTime.With(DamageSource.Secondary).Value.Invert
                },
                {
                    BaseSet, MetaStats.CastRate,
                    CombineSourceDefaultingToSpell(Stat.CastRate, CombineHandsByAverage)
                },
                { BaseAdd, MetaStats.CastRate, Stat.AdditionalCastRate.Value },
                { TotalOverride, MetaStats.CastTime, MetaStats.CastRate.Value.Invert },
                { PercentMore, Stat.MovementSpeed, ActionSpeedValueForPercentMore },
                {
                    PercentMore, Stat.CastRate, ActionSpeedValueForPercentMore,
                    Not(Or(With(Keyword.Totem), With(Keyword.Trap), With(Keyword.Mine)))
                },
                { PercentMore, Stat.Totem.Speed, ActionSpeedValueForPercentMore },
                { PercentMore, Stat.Trap.Speed, ActionSpeedValueForPercentMore },
                { PercentMore, Stat.Mine.Speed, ActionSpeedValueForPercentMore },
                // resistances/damage reduction
                { BaseSet, MetaStats.ResistanceAgainstHits(DamageType.Physical), Physical.Resistance.Value },
                {
                    BaseAdd, MetaStats.ResistanceAgainstHits(DamageType.Physical),
                    100 * Armour.Value /
                    (Armour.Value + 10 * Physical.Damage.WithSkills.With(AttackDamageHand.MainHand).For(Enemy).Value)
                },
                { BaseSet, MetaStats.ResistanceAgainstHits(DamageType.Physical).Maximum, 90 },
                { TotalOverride, MetaStats.ResistanceAgainstHits(DamageType.Lightning), Lightning.Resistance.Value },
                { TotalOverride, MetaStats.ResistanceAgainstHits(DamageType.Cold), Cold.Resistance.Value },
                { TotalOverride, MetaStats.ResistanceAgainstHits(DamageType.Fire), Fire.Resistance.Value },
                { TotalOverride, MetaStats.ResistanceAgainstHits(DamageType.Chaos), Chaos.Resistance.Value },
                {
                    BaseAdd, dt => DamageTypeBuilders.From(dt).Resistance,
                    dt => DamageTypeBuilders.From(dt).Exposure.Value
                },
                // damage mitigation (1 - (1 - resistance / 100) * damage taken)
                {
                    TotalOverride, MetaStats.MitigationAgainstHits,
                    dt => 1 - DamageTakenMultiplier(MetaStats.ResistanceAgainstHits(dt),
                              DamageTaken(dt).WithSkills(DamageSource.Secondary))
                },
                {
                    TotalOverride, MetaStats.MitigationAgainstDoTs,
                    dt => 1 - DamageTakenMultiplier(DamageTypeBuilders.From(dt).Resistance,
                              DamageTaken(dt).WithSkills(DamageSource.OverTime))
                },
                // chance to hit/evade
                {
                    BaseSet, Evasion.Chance,
                    100 - ChanceToHitValue(Stat.Accuracy.With(AttackDamageHand.MainHand).For(Enemy), Evasion,
                        Buff.Blind.IsOn(Enemy))
                },
                {
                    BaseSet, Stat.ChanceToHit.With(AttackDamageHand.MainHand),
                    ChanceToHitValue(Stat.Accuracy.With(AttackDamageHand.MainHand), Evasion.For(Enemy),
                        Buff.Blind.IsOn(Self))
                },
                {
                    BaseSet, Stat.ChanceToHit.With(AttackDamageHand.OffHand),
                    ChanceToHitValue(Stat.Accuracy.With(AttackDamageHand.OffHand), Evasion.For(Enemy),
                        Buff.Blind.IsOn(Self))
                },
                // chance to avoid
                {
                    TotalOverride, MetaStats.ChanceToAvoidMeleeAttacks,
                    100 - 100 * (FailureProbability(Evasion.ChanceAgainstMeleeAttacks) *
                                 FailureProbability(Stat.Dodge.AttackChance) * FailureProbability(Block.AttackChance))
                },
                {
                    TotalOverride, MetaStats.ChanceToAvoidProjectileAttacks,
                    100 - 100 * (FailureProbability(Evasion.ChanceAgainstProjectileAttacks) *
                                 FailureProbability(Stat.Dodge.AttackChance) * FailureProbability(Block.AttackChance))
                },
                {
                    TotalOverride, MetaStats.ChanceToAvoidSpells,
                    100 - 100 * (FailureProbability(Stat.Dodge.SpellChance) * FailureProbability(Block.SpellChance))
                },
                // crit
                {
                    TotalOverride, MetaStats.EffectiveCritChance.With(AttackDamageHand.MainHand),
                    CalculateLuckyCriticalStrikeChance(CriticalStrike.Chance.With(AttackDamageHand.MainHand)) *
                    Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.EffectiveCritChance.With(AttackDamageHand.OffHand),
                    CalculateLuckyCriticalStrikeChance(CriticalStrike.Chance.With(AttackDamageHand.OffHand)) *
                    Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.EffectiveCritChance.With(DamageSource.Spell),
                    CalculateLuckyCriticalStrikeChance(CriticalStrike.Chance.With(DamageSource.Spell))
                },
                {
                    TotalOverride, MetaStats.EffectiveCritChance.With(DamageSource.Secondary),
                    CalculateLuckyCriticalStrikeChance(CriticalStrike.Chance.With(DamageSource.Secondary))
                },
                // pools
                {
                    BaseAdd, p => Stat.Pool.From(p).Regen,
                    p => MetaStats.RegenTargetPoolValue(p) * Stat.Pool.From(p).Regen.Percent.Value.AsPercentage
                },
                { TotalOverride, MetaStats.EffectiveRegen, p => p.Regen.Value * p.RecoveryRate.Value },
                { TotalOverride, MetaStats.EffectiveRecharge, p => p.Recharge.Value * p.RecoveryRate.Value },
                { TotalOverride, MetaStats.RechargeStartDelay, p => 2 / p.Recharge.Start.Value },
                { TotalOverride, MetaStats.EffectiveLeechRate, p => p.Leech.Rate.Value * p.RecoveryRate.Value },
                {
                    TotalOverride, MetaStats.AbsoluteLeechRate,
                    p => Stat.Pool.From(p).Value * MetaStats.EffectiveLeechRate(p).Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.AbsoluteLeechRateLimit,
                    p => Stat.Pool.From(p).Value * Stat.Pool.From(p).Leech.RateLimit.Value.AsPercentage
                },
                {
                    TotalOverride, MetaStats.TimeToReachLeechRateLimit,
                    p => p.Leech.RateLimit.Value / p.Leech.Rate.Value /
                         (MetaStats.CastRate.Value * MetaStats.SkillNumberOfHitsPerCast.Value)
                },
                // flasks
                { PercentMore, Flask.LifeRecovery, Flask.Effect.Value * 100 },
                { PercentMore, Flask.ManaRecovery, Flask.Effect.Value * 100 },
                { PercentMore, Flask.LifeRecovery, Flask.LifeRecoverySpeed.Value * 100 },
                { PercentMore, Flask.ManaRecovery, Flask.ManaRecoverySpeed.Value * 100 },
                // ailments
                {
                    TotalOverride, MetaStats.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Ignite),
                    (int) DamageType.Fire
                },
                {
                    TotalOverride, MetaStats.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Bleed),
                    (int) DamageType.Physical
                },
                {
                    TotalOverride, MetaStats.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Poison),
                    (int) DamageType.Chaos
                },
                {
                    TotalOverride, MetaStats.AilmentCombinedEffectiveChance,
                    ailment => CombineSource(MetaStats.AilmentEffectiveChance(ailment), CombineHandsByAverage)
                },
                {
                    TotalOverride, MetaStats.AilmentEffectiveChance,
                    ailment => Ailment.From(ailment).Chance,
                    ailment => MetaStats.AilmentChanceWithCrits(ailment),
                    _ => MetaStats.EffectiveCritChance,
                    (ailment, ailmentChance, ailmentChanceWithCrits, critChance)
                        => (ailmentChance.Value.AsPercentage * (1 - critChance.Value) +
                            ailmentChanceWithCrits.Value.AsPercentage * critChance.Value) *
                           (1 - Ailment.From(ailment).Avoidance.For(Enemy).Value.AsPercentage)
                },
                {
                    TotalOverride, MetaStats.AilmentChanceWithCrits,
                    ailment => Ailment.From(ailment).Chance,
                    (ailment, ailmentChance) => ValueFactory
                        .If(Ailment.From(ailment).CriticalStrikesAlwaysInflict.IsSet).Then(100)
                        .Else(ailmentChance.Value)
                },
                { TotalOverride, Ailment.Chill.On(Self), 1, Ailment.Freeze.IsOn(Self) },
                {
                    PercentIncrease, Ailment.Shock.AddStat(Damage.Taken), MetaStats.IncreasedDamageTakenFromShocks.Value
                },
                { TotalOverride, MetaStats.IncreasedDamageTakenFromShocks.Maximum, 50 },
                { TotalOverride, MetaStats.IncreasedDamageTakenFromShocks.Minimum, 1 },
                {
                    PercentReduce, Ailment.Chill.AddStat(Stat.ActionSpeed),
                    MetaStats.ReducedActionSpeedFromChill.Value
                },
                { TotalOverride, MetaStats.ReducedActionSpeedFromChill.Maximum, 30 },
                { TotalOverride, MetaStats.ReducedActionSpeedFromChill.Minimum, 1 },
                { BaseSet, a => Ailment.From(a).TickRateModifier, a => ValueFactory.Create(1) },
                { PercentMore, a => Ailment.From(a).Duration, a => 100 / Ailment.From(a).TickRateModifier.Value },
                // - AilmentEffectiveInstances
                {
                    TotalOverride, MetaStats.AilmentEffectiveInstances(Common.Builders.Effects.Ailment.Ignite),
                    Ailment.Ignite.InstancesOn(Enemy).Maximum.Value
                },
                {
                    TotalOverride, MetaStats.AilmentEffectiveInstances(Common.Builders.Effects.Ailment.Bleed),
                    Ailment.Bleed.InstancesOn(Enemy).Maximum.Value
                },
                {
                    TotalOverride, MetaStats.AilmentEffectiveInstances(Common.Builders.Effects.Ailment.Poison),
                    Ailment.Poison.Duration.Value * MetaStats.CastRate.Value *
                    MetaStats.SkillNumberOfHitsPerCast.Value *
                    CombineSource(MetaStats.AilmentEffectiveChance(Common.Builders.Effects.Ailment.Poison),
                        CombineHandsForAilmentEffectiveInstances(Common.Builders.Effects.Ailment.Poison))
                },
                // buffs
                {
                    PercentMore,
                    MetaStats.EffectiveDamageMultiplierWithNonCrits(DamageType.Physical).WithSkills,
                    Buff.Impale.Chance,
                    chance => ValueFactory.If(Buff.Impale.IsOn(Self, Enemy))
                        .Then(10 * Buff.Impale.EffectOn(Enemy).Value * Buff.Impale.StackCount.For(Enemy).Value
                              * chance.WithCondition(Hit.On).Value.AsPercentage)
                        .Else(0)
                },
                {
                    PercentMore,
                    MetaStats.EffectiveDamageMultiplierWithCrits(DamageType.Physical).WithSkills,
                    Buff.Impale.Chance,
                    chance => ValueFactory.If(Buff.Impale.IsOn(Self, Enemy))
                        .Then(10 * Buff.Impale.EffectOn(Enemy).Value * Buff.Impale.StackCount.For(Enemy).Value
                              * chance.WithCondition(Hit.On).Value.AsPercentage)
                        .Else(0)
                },
                { TotalOverride, Buff.Impale.Chance.WithCondition(Hit.On).Maximum, 100 },
                // stun (see https://pathofexile.gamepedia.com/Stun)
                { PercentMore, Effect.Stun.Duration, 100 / Effect.Stun.Recovery.For(Enemy).Value - 100 },
                {
                    TotalOverride, MetaStats.EffectiveStunThreshold,
                    Effect.Stun.Threshold, EffectiveStunThresholdValue
                },
                {
                    BaseSet, Effect.Stun.Chance,
                    MetaStats.AverageDamage.WithHits, MetaStats.EffectiveStunThreshold.For(Enemy),
                    (damage, threshold)
                        => 200 * damage.Value / (Life.For(Enemy).ValueFor(NodeType.Subtotal) * threshold.Value)
                },
                {
                    TotalOverride, MetaStats.StunAvoidanceWhileCasting,
                    1 -
                    (1 - Effect.Stun.Avoidance.Value) * (1 - Effect.Stun.ChanceToAvoidInterruptionWhileCasting.Value)
                },
                // flags
                {
                    PercentMore, Damage.WithSkills(DamageSource.Attack).With(Keyword.Projectile),
                    30 * ValueFactory.LinearScale(Projectile.TravelDistance, (35, 0), (70, 1)),
                    Flag.FarShot.IsSet
                },
                // other
                { PercentMore, Stat.Radius, Stat.AreaOfEffect.Value.Select(Math.Sqrt, v => $"Sqrt({v})") },
                { PercentMore, Stat.Cooldown, 100 - 100 * Stat.CooldownRecoverySpeed.Value.Invert },
                { BaseSet, MetaStats.SkillNumberOfHitsPerCast, 1 },
                { BaseSet, Stat.MainSkillPart, 0 },
            };

        private static ValueBuilder AverageAilmentDamageFromCritAndNonCrit(
            IStatBuilder nonCritDamage, IStatBuilder critDamage, IStatBuilder critChance,
            IStatBuilder nonCritAilmentChance, IStatBuilder critAilmentChance)
        {
            return CombineByWeightedAverage(
                nonCritDamage.Value.Average, (1 - critChance.Value) * nonCritAilmentChance.Value.AsPercentage,
                critDamage.Value.Average, critChance.Value * critAilmentChance.Value.AsPercentage);
        }

        private ValueBuilder EnemyDamageTakenMultiplier(DamageType resistanceType, IStatBuilder damageTaken)
            => DamageTakenMultiplier(DamageTypeBuilders.From(resistanceType).Resistance.For(Enemy),
                damageTaken.For(Enemy));

        private static ValueBuilder DamageTakenMultiplier(IStatBuilder resistance, IStatBuilder damageTaken)
            => (1 - resistance.Value.AsPercentage) * damageTaken.Value;

        private IDamageRelatedStatBuilder DamageTaken(DamageType damageType)
            => DamageTypeBuilders.From(damageType).Damage.Taken;

        private IDamageRelatedStatBuilder DamageMultiplier(DamageType damageType)
            => DamageTypeBuilders.From(damageType).DamageMultiplier;

        private ValueBuilder ActionSpeedValueForPercentMore => (Stat.ActionSpeed.Value - 1) * 100;

        private ValueBuilder ChanceToHitValue(
            IStatBuilder accuracyStat, IStatBuilder evasionStat, IConditionBuilder isBlinded)
        {
            var accuracy = accuracyStat.Value;
            var evasion = evasionStat.Value;
            var blindMultiplier = ValueFactory.If(isBlinded).Then(0.5).Else(1);
            return 100 * blindMultiplier * 1.15 * accuracy /
                   (accuracy + (evasion / 4).Select(d => Math.Pow(d, 0.8), v => $"{v}^0.8"));
        }

        private static ValueBuilder FailureProbability(IStatBuilder percentageChanceStat)
            => 1 - percentageChanceStat.Value.AsPercentage;

        private IValueBuilder EffectiveStunThresholdValue(IStatBuilder stunThresholdStat)
        {
            // If stun threshold is less than 25%, it is scaled up.
            // See https://pathofexile.gamepedia.com/Stun#Stun_threshold
            var stunThreshold = stunThresholdStat.Value;
            return ValueFactory
                .If(stunThreshold >= 0.25).Then(stunThreshold)
                .Else(0.25 - 0.25 * (0.25 - stunThreshold) / (0.5 - stunThreshold));
        }

        private IReadOnlyList<IIntermediateModifier> CollectionToList(DataDrivenMechanicCollection collection)
        {
            AddDamageWithNonCritsModifiers(collection);
            AddDamageWithCritsModifiers(collection);
            AddAilmentEffectiveDamageMultiplierModifiers(collection);
            AddAilmentSourceDamageTypeModifiers(collection);
            return collection.ToList();
        }

        private void AddAilmentEffectiveDamageMultiplierModifiers(DataDrivenMechanicCollection collection)
        {
            var ailmentsAndTypes = new[]
            {
                (Common.Builders.Effects.Ailment.Ignite, DamageType.Fire),
                (Common.Builders.Effects.Ailment.Bleed, DamageType.Physical),
                (Common.Builders.Effects.Ailment.Poison, DamageType.Chaos),
            };
            foreach (var (ailment, damageType) in ailmentsAndTypes)
            {
                AddEffectiveDamageMultiplierWithNonCritsModifiers(collection, ailment, damageType);
                AddEffectiveDamageMultiplierWithCritsModifiers(collection, ailment, damageType);
            }
        }

        private void AddEffectiveDamageMultiplierWithNonCritsModifiers(
            DataDrivenMechanicCollection collection, Ailment ailment, DamageType damageType)
        {
            var ailmentBuilder = Ailment.From(ailment);
            collection.Add(BaseSet, dt => MetaStats.EffectiveDamageMultiplierWithNonCrits(dt).With(ailmentBuilder),
                _ => DamageTaken(damageType).With(ailmentBuilder),
                _ => DamageMultiplier(damageType).With(ailmentBuilder),
                (_, damageTaken, damageMulti)
                    => EnemyDamageTakenMultiplier(damageType, damageTaken) * damageMulti.Value.AsPercentage);
        }

        private void AddEffectiveDamageMultiplierWithCritsModifiers(
            DataDrivenMechanicCollection collection, Ailment ailment, DamageType damageType)
        {
            var ailmentBuilder = Ailment.From(ailment);
            collection.Add(BaseSet, dt => MetaStats.EffectiveDamageMultiplierWithCrits(dt).With(ailmentBuilder),
                _ => DamageTaken(damageType).With(ailmentBuilder),
                _ => CriticalStrike.Multiplier.With(ailmentBuilder),
                _ => DamageMultiplier(damageType).With(ailmentBuilder),
                (_, damageTaken, damageMulti, critMulti)
                    => EnemyDamageTakenMultiplier(damageType, damageTaken) * damageMulti.Value.AsPercentage
                                                                           * critMulti.Value.AsPercentage);
        }

        private void AddAilmentSourceDamageTypeModifiers(GivenStatCollection collection)
        {
            foreach (var ailment in Enums.GetValues<Ailment>())
            {
                var ailmentBuilder = Ailment.From(ailment);
                foreach (var damageType in Enums.GetValues<DamageType>())
                {
                    collection.Add(TotalOverride, MetaStats.Damage(damageType).With(ailmentBuilder), 0,
                        ailmentBuilder.Source(DamageTypeBuilders.From(damageType)).IsSet.Not);
                }
            }
        }

        private void AddDamageWithNonCritsModifiers(GivenStatCollection collection)
        {
            AddDamageWithModifiers(collection, MetaStats.DamageWithNonCrits(), MetaStats.DamageWithNonCrits);
        }

        private void AddDamageWithCritsModifiers(GivenStatCollection collection)
        {
            AddDamageWithModifiers(collection, MetaStats.DamageWithCrits(), MetaStats.DamageWithCrits);
        }

        private void AddDamageWithModifiers(GivenStatCollection collection,
            IDamageRelatedStatBuilder damage, Func<DamageType, IDamageRelatedStatBuilder> damageForType)
        {
            var form = BaseAdd;
            foreach (var type in Enums.GetValues<DamageType>().Except(DamageType.RandomElement))
            {
                var forType = damageForType(type);
                AddForSkillAndAilments(collection, form, damage.With(AttackDamageHand.MainHand),
                    forType.With(AttackDamageHand.MainHand));
                AddForSkillAndAilments(collection, form, damage.With(AttackDamageHand.OffHand),
                    forType.With(AttackDamageHand.OffHand));
                AddForSkillAndAilments(collection, form, damage.With(DamageSource.Spell),
                    forType.With(DamageSource.Spell));
                AddForSkillAndAilments(collection, form, damage.With(DamageSource.Secondary),
                    forType.With(DamageSource.Secondary));
                collection.Add(form, damage.WithSkills(DamageSource.OverTime),
                    forType.WithSkills(DamageSource.OverTime).Value);
            }
        }

        private void AddForSkillAndAilments(GivenStatCollection collection,
            IFormBuilder form, IDamageRelatedStatBuilder stat, IDamageRelatedStatBuilder valueStat)
        {
            collection.Add(form, stat.WithSkills, valueStat.WithSkills.Value);
            foreach (var ailment in Enums.GetValues<Ailment>())
            {
                var ailmentBuilder = Ailment.From(ailment);
                collection.Add(form, stat.With(ailmentBuilder), valueStat.With(ailmentBuilder).Value);
            }
        }

        private ValueBuilder CalculateLuckyCriticalStrikeChance(IStatBuilder critChance)
        {
            var critValue = critChance.Value.AsPercentage;
            return ValueFactory.If(Flag.CriticalStrikeChanceIsLucky.IsSet)
                .Then(1 - (1 - critValue) * (1 - critValue))
                .Else(critValue);
        }

        private ValueBuilder CombineSource(
            IDamageRelatedStatBuilder statToCombine, Func<IDamageRelatedStatBuilder, IValueBuilder> handCombiner)
            => ValueFactory.If(MetaStats.SkillHitDamageSource.Value.Eq((int) DamageSource.Attack))
                .Then(handCombiner(statToCombine))
                .ElseIf(MetaStats.SkillHitDamageSource.Value.Eq((int) DamageSource.Spell))
                .Then(statToCombine.With(DamageSource.Spell).Value)
                .ElseIf(MetaStats.SkillHitDamageSource.Value.Eq((int) DamageSource.Secondary))
                .Then(statToCombine.With(DamageSource.Secondary).Value)
                .Else(0);

        private ValueBuilder CombineSourceDefaultingToSpell(
            IDamageRelatedStatBuilder statToCombine, Func<IDamageRelatedStatBuilder, IValueBuilder> handCombiner)
            => ValueFactory.If(MetaStats.SkillHitDamageSource.Value.Eq((int) DamageSource.Attack))
                .Then(handCombiner(statToCombine))
                .ElseIf(MetaStats.SkillHitDamageSource.Value.Eq((int) DamageSource.Secondary))
                .Then(statToCombine.With(DamageSource.Secondary).Value)
                .Else(statToCombine.With(DamageSource.Spell).Value);

        private ValueBuilder CombineHandsByAverage(IDamageRelatedStatBuilder statToCombine)
        {
            var mhWeight = SkillUsesHandAsMultiplier(AttackDamageHand.MainHand);
            var ohWeight = SkillUsesHandAsMultiplier(AttackDamageHand.OffHand);
            return CombineByWeightedAverage(
                statToCombine.With(AttackDamageHand.MainHand).Value, mhWeight,
                statToCombine.With(AttackDamageHand.OffHand).Value, ohWeight);
        }

        private Func<IDamageRelatedStatBuilder, ValueBuilder> CombineHandsForAverageAilmentDamage(
            Ailment ailment)
        {
            var ailmentChance = MetaStats.AilmentEffectiveChance(ailment);
            var mhWeight = CalculateAilmentHandWeight(ailmentChance, AttackDamageHand.MainHand);
            var ohWeight = CalculateAilmentHandWeight(ailmentChance, AttackDamageHand.OffHand);
            return statToCombine =>
            {
                var mhDamage = statToCombine.With(AttackDamageHand.MainHand).Value;
                var ohDamage = statToCombine.With(AttackDamageHand.OffHand).Value;
                return CombineByWeightedAverage(
                    mhDamage, ValueFactory.If(mhDamage > 0).Then(mhWeight).Else(0),
                    ohDamage, ValueFactory.If(ohDamage > 0).Then(ohWeight).Else(0));
            };
        }

        private ValueBuilder CalculateAilmentHandWeight(IDamageRelatedStatBuilder ailmentChance, AttackDamageHand hand)
            => ailmentChance.With(hand).Value *
               Stat.ChanceToHit.With(hand).Value.AsPercentage *
               SkillUsesHandAsMultiplier(hand);

        private Func<IDamageRelatedStatBuilder, ValueBuilder> CombineHandsForAilmentEffectiveInstances(
            Ailment ailment)
        {
            var ailmentDamage = MetaStats.AverageDamage.With(Ailment.From(ailment));
            var mhDamage = ailmentDamage.With(AttackDamageHand.MainHand).Value;
            var ohDamage = ailmentDamage.With(AttackDamageHand.OffHand).Value;
            var mhWeight = SkillUsesHandAsMultiplier(AttackDamageHand.MainHand);
            var ohWeight = SkillUsesHandAsMultiplier(AttackDamageHand.OffHand);
            return s => CombineByWeightedAverage(
                s.With(AttackDamageHand.MainHand).Value *
                Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value.AsPercentage,
                ValueFactory.If(mhDamage > 0).Then(mhWeight).Else(0),
                s.With(AttackDamageHand.OffHand).Value *
                Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value.AsPercentage,
                ValueFactory.If(ohDamage > 0).Then(ohWeight).Else(0));
        }

        private ValueBuilder CombineHandsForHitDamage(IDamageRelatedStatBuilder statToCombine)
        {
            var usesMh = SkillUsesHandAsMultiplier(AttackDamageHand.MainHand);
            var usesOh = SkillUsesHandAsMultiplier(AttackDamageHand.OffHand);
            var sumOfHands = statToCombine.With(AttackDamageHand.MainHand).Value * usesMh +
                             statToCombine.With(AttackDamageHand.OffHand).Value * usesOh;
            return ValueFactory.If(MetaStats.SkillDoubleHitsWhenDualWielding.IsSet)
                .Then(sumOfHands)
                .Else(sumOfHands / (usesMh + usesOh));
        }

        private static ValueBuilder CombineByWeightedAverage(
            ValueBuilder left, ValueBuilder leftWeight, ValueBuilder right, ValueBuilder rightWeight)
            => (left * leftWeight + right * rightWeight) / (leftWeight + rightWeight);

        private ValueBuilder SkillUsesHandAsMultiplier(AttackDamageHand hand)
            => ValueFactory.If(MetaStats.SkillUsesHand(hand).IsSet).Then(1).Else(0);
    }
}