using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.GivenStats
{
    public class DataDrivenMechanics : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly IMetaStatBuilders _stat;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public DataDrivenMechanics(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, IMetaStatBuilders metaStatBuilders)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _stat = metaStatBuilders;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new string[0];

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private IEnumerable<IIntermediateModifier> CreateCollection()
            => new DataDrivenMechanicCollection(_modifierBuilder, BuilderFactories)
            {
                // skill hit damage
                // - DPS
                { TotalOverride, _stat.SkillDpsWithHits, _stat.AverageHitDamage.Value * _stat.CastRate.Value },
                // - average damage
                {
                    TotalOverride, _stat.AverageHitDamage,
                    CombineSource(_stat.AverageDamage.WithHits, CombineHandsByAverage)
                },
                // - average damage per source
                {
                    TotalOverride, _stat.AverageDamage.WithHits.With(AttackDamageHand.MainHand),
                    _stat.AverageDamagePerHit.With(AttackDamageHand.MainHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value
                },
                {
                    TotalOverride, _stat.AverageDamage.WithHits.With(AttackDamageHand.OffHand),
                    _stat.AverageDamagePerHit.With(AttackDamageHand.OffHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value
                },
                {
                    TotalOverride, _stat.AverageDamage.WithHits.With(DamageSource.Spell),
                    _stat.AverageDamagePerHit.With(DamageSource.Spell).Value
                },
                {
                    TotalOverride, _stat.AverageDamage.WithHits.With(DamageSource.Secondary),
                    _stat.AverageDamagePerHit.With(DamageSource.Secondary).Value
                },
                // - average damage of a successful hit per source
                {
                    TotalOverride, _stat.AverageDamagePerHit,
                    _stat.DamageWithNonCrits().WithHits, _stat.DamageWithCrits().WithHits, _stat.EffectiveCritChance,
                    (nonCritDamage, critDamage, critChance)
                        => nonCritDamage.Value.Average * (1 - critChance.Value) +
                           critDamage.Value.Average * critChance.Value
                },
                // - critical/non-critical damage per source
                { BaseAdd, _stat.DamageWithNonCrits().WithHits, dt => _stat.DamageWithNonCrits(dt).WithHits },
                { BaseAdd, _stat.DamageWithCrits().WithHits, dt => _stat.DamageWithCrits(dt).WithHits },
                // - crit/non-crit damage per source and type
                {
                    TotalOverride, dt => _stat.DamageWithNonCrits(dt).WithHits,
                    dt => DamageTypeBuilders.From(dt).Damage.WithHits,
                    dt => _stat.EffectiveDamageMultiplierWithNonCrits(dt).WithHits,
                    (_, damage, mult) => damage.Value * mult.Value
                },
                {
                    TotalOverride, dt => _stat.DamageWithCrits(dt).WithHits,
                    dt => DamageTypeBuilders.From(dt).Damage.WithHits,
                    dt => _stat.EffectiveDamageMultiplierWithCrits(dt).WithHits,
                    (_, damage, mult) => damage.Value * mult.Value
                },
                // - effective crit/non-crit damage multiplier per source and type
                {
                    TotalOverride, dt => _stat.EffectiveDamageMultiplierWithNonCrits(dt).WithHits,
                    dt => _stat.EnemyResistanceAgainstNonCrits(dt),
                    dt => DamageTypeBuilders.From(dt).Damage.Taken.For(Enemy),
                    (_, resistance, damageTaken) => (1 - resistance.Value / 100) * damageTaken.Value
                },
                {
                    TotalOverride, dt => _stat.EffectiveDamageMultiplierWithCrits(dt).WithHits,
                    dt => _stat.EnemyResistanceAgainstCrits(dt),
                    dt => DamageTypeBuilders.From(dt).Damage.Taken.For(Enemy),
                    _ => CriticalStrike.Multiplier.WithHits,
                    (_, resistance, damageTaken, mult) => (1 - resistance.Value / 100) * damageTaken.Value * mult.Value
                },
                // - enemy resistance against crit/non-crit hits per source and type
                {
                    TotalOverride, dt => _stat.EnemyResistanceAgainstNonCrits(dt),
                    dt => DamageTypeBuilders.From(dt).IgnoreResistanceWithNonCrits,
                    dt => DamageTypeBuilders.From(dt).PenetrationWithNonCrits,
                    (dt, ignoreResistance, penetration)
                        => ValueFactory.If(ignoreResistance.IsSet).Then(0)
                            .Else(DamageTypeBuilders.From(dt).Resistance.For(Enemy).Value - penetration.Value)
                },
                {
                    TotalOverride, dt => _stat.EnemyResistanceAgainstCrits(dt),
                    dt => DamageTypeBuilders.From(dt).IgnoreResistanceWithCrits,
                    dt => DamageTypeBuilders.From(dt).PenetrationWithCrits,
                    (dt, ignoreResistance, penetration)
                        => ValueFactory.If(ignoreResistance.Value.Eq(1)).Then(0)
                            .Else(DamageTypeBuilders.From(dt).Resistance.For(Enemy).Value - penetration.Value)
                },

                // skill damage over time
                // - DPS = average damage = non-crit damage
                { TotalOverride, _stat.SkillDpsWithDoTs, _stat.AverageDamage.With(DamageSource.OverTime).Value },
                {
                    TotalOverride, _stat.AverageDamage.With(DamageSource.OverTime),
                    _stat.DamageWithNonCrits().With(DamageSource.OverTime).Value
                },
                {
                    BaseAdd, _ => _stat.DamageWithNonCrits().With(DamageSource.OverTime),
                    dt => _stat.DamageWithNonCrits(dt).With(DamageSource.OverTime).Value
                },
                // - damage per type
                {
                    TotalOverride, dt => _stat.DamageWithNonCrits(dt).With(DamageSource.OverTime),
                    dt => DamageTypeBuilders.From(dt).Damage.With(DamageSource.OverTime).Value *
                          _stat.EffectiveDamageMultiplierWithNonCrits(dt).With(DamageSource.OverTime).Value
                },
                // - effective damage multiplier per type
                {
                    TotalOverride, dt => _stat.EffectiveDamageMultiplierWithNonCrits(dt).With(DamageSource.OverTime),
                    dt => (1 - DamageTypeBuilders.From(dt).Resistance.For(Enemy).Value / 100) *
                          DamageTypeBuilders.From(dt).Damage.Taken.With(DamageSource.OverTime).For(Enemy).Value
                },

                // ignite damage
                // - DPS
                {
                    TotalOverride, _stat.IgniteDps,
                    _stat.AverageIgniteDamage.Value * Ailment.Ignite.InstancesOn(Enemy).Maximum.Value
                },
                // - average damage
                {
                    TotalOverride, _stat.AverageIgniteDamage,
                    CombineSource(_stat.AverageDamage.With(Ailment.Ignite),
                        CombineHandsByWeightedAverage(
                            Stat.ChanceToHit, _stat.AilmentEffectiveChance(Common.Builders.Effects.Ailment.Ignite)))
                },
                // - average damage per source
                {
                    TotalOverride, _stat.AverageDamage.With(Ailment.Ignite),
                    _stat.DamageWithNonCrits().With(Ailment.Ignite), _stat.DamageWithCrits().With(Ailment.Ignite),
                    _stat.EffectiveCritChance,
                    Ailment.Ignite.Chance, _stat.AilmentChanceWithCrits(Common.Builders.Effects.Ailment.Ignite),
                    (nonCritDamage, critDamage, critChance, nonCritIgniteChance, critIgniteChance)
                        => CombineByWeightedAverage(
                            nonCritDamage.Value.Average, (1 - critChance.Value) * nonCritIgniteChance.Value / 100,
                            critDamage.Value.Average, critChance.Value * critIgniteChance.Value / 100)
                },
                // - crit/non-crit damage per source
                {
                    BaseAdd, _stat.DamageWithNonCrits().With(Ailment.Ignite),
                    dt => _stat.DamageWithNonCrits(dt).With(Ailment.Ignite)
                },
                {
                    BaseAdd, _stat.DamageWithCrits().With(Ailment.Ignite),
                    dt => _stat.DamageWithCrits(dt).With(Ailment.Ignite)
                },
                // - crit/non-crit damage per source and type
                {
                    TotalOverride, dt => _stat.DamageWithNonCrits(dt).With(Ailment.Ignite),
                    dt => DamageTypeBuilders.From(dt).Damage.With(Ailment.Ignite),
                    dt => _stat.EffectiveDamageMultiplierWithNonCrits(dt).With(Ailment.Ignite),
                    (_, damage, mult) => damage.Value * mult.Value
                },
                {
                    TotalOverride, dt => _stat.DamageWithCrits(dt).With(Ailment.Ignite),
                    dt => DamageTypeBuilders.From(dt).Damage.With(Ailment.Ignite),
                    dt => _stat.EffectiveDamageMultiplierWithCrits(dt).With(Ailment.Ignite),
                    (_, damage, mult) => damage.Value * mult.Value
                },
                // - effective crit/non-crit damage multiplier per source and type
                {
                    TotalOverride, dt => _stat.EffectiveDamageMultiplierWithNonCrits(dt).With(Ailment.Ignite),
                    _ => Fire.Damage.Taken.With(Ailment.Ignite).For(Enemy),
                    (_, damageTaken) => (1 - Fire.Resistance.For(Enemy).Value / 100) * damageTaken.Value
                },
                {
                    TotalOverride, dt => _stat.EffectiveDamageMultiplierWithCrits(dt).With(Ailment.Ignite),
                    _ => Fire.Damage.Taken.With(Ailment.Ignite).For(Enemy),
                    _ => CriticalStrike.Multiplier.With(Ailment.Ignite),
                    (_, damageTaken, mult)
                        => (1 - Fire.Resistance.For(Enemy).Value / 100) * damageTaken.Value * mult.Value
                },

                // speed
                {
                    // This assumes cast rate is only set for the skill's hit damage source.
                    TotalOverride, _stat.CastRate,
                    (Stat.CastRate.With(AttackDamageHand.MainHand).Value +
                     Stat.CastRate.With(AttackDamageHand.OffHand).Value) / 2
                    + Stat.CastRate.With(DamageSource.Spell).Value + Stat.CastRate.With(DamageSource.Secondary).Value
                },
                { TotalOverride, _stat.CastTime, _stat.CastRate.Value.Invert },
                // resistances/damage reduction
                { BaseSet, _stat.ResistanceAgainstHits(DamageType.Physical), Physical.Resistance.Value },
                {
                    BaseAdd, _stat.ResistanceAgainstHits(DamageType.Physical),
                    100 * Armour.Value /
                    (Armour.Value + 10 * Physical.Damage.With(AttackDamageHand.MainHand).For(Enemy).Value)
                },
                { BaseSet, _stat.ResistanceAgainstHits(DamageType.Physical).Maximum, 90 },
                { TotalOverride, _stat.ResistanceAgainstHits(DamageType.Lightning), Lightning.Resistance.Value },
                { TotalOverride, _stat.ResistanceAgainstHits(DamageType.Cold), Cold.Resistance.Value },
                { TotalOverride, _stat.ResistanceAgainstHits(DamageType.Fire), Fire.Resistance.Value },
                { TotalOverride, _stat.ResistanceAgainstHits(DamageType.Chaos), Chaos.Resistance.Value },
                // damage mitigation (1 - (1 - resistance / 100) * damage taken)
                {
                    TotalOverride, _stat.MitigationAgainstHits,
                    dt => 1 - (1 - _stat.ResistanceAgainstHits(dt).Value / 100) *
                          DamageTypeBuilders.From(dt).Damage.Taken.With(DamageSource.Secondary).Value
                },
                {
                    TotalOverride, _stat.MitigationAgainstDoTs,
                    dt => 1 - (1 - DamageTypeBuilders.From(dt).Resistance.Value / 100) *
                          DamageTypeBuilders.From(dt).Damage.Taken.With(DamageSource.OverTime).Value
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
                    TotalOverride, _stat.ChanceToAvoidMeleeAttacks,
                    100 - 100 * (FailureProbability(Evasion.ChanceAgainstMeleeAttacks) *
                                 FailureProbability(Stat.Dodge.AttackChance) * FailureProbability(Block.AttackChance))
                },
                {
                    TotalOverride, _stat.ChanceToAvoidProjectileAttacks,
                    100 - 100 * (FailureProbability(Evasion.ChanceAgainstProjectileAttacks) *
                                 FailureProbability(Stat.Dodge.AttackChance) * FailureProbability(Block.AttackChance))
                },
                {
                    TotalOverride, _stat.ChanceToAvoidSpells,
                    100 - 100 * (FailureProbability(Stat.Dodge.SpellChance) * FailureProbability(Block.SpellChance))
                },
                // crit
                {
                    TotalOverride, _stat.EffectiveCritChance.With(AttackDamageHand.MainHand),
                    CriticalStrike.Chance.With(AttackDamageHand.MainHand).Value / 100 *
                    Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(AttackDamageHand.OffHand),
                    CriticalStrike.Chance.With(AttackDamageHand.OffHand).Value / 100 *
                    Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(DamageSource.Spell),
                    CriticalStrike.Chance.With(DamageSource.Spell).Value / 100
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(DamageSource.Secondary),
                    CriticalStrike.Chance.With(DamageSource.Secondary).Value / 100
                },
                // pools
                { BaseAdd, p => p.Regen, p => _stat.RegenTargetPoolValue(p.BuildPool()) * p.Regen.Percent.Value / 100 },
                { TotalOverride, _stat.EffectiveRegen, p => p.Regen.Value * p.RecoveryRate.Value },
                { TotalOverride, _stat.EffectiveRecharge, p => p.Recharge.Value * p.RecoveryRate.Value },
                { TotalOverride, _stat.RechargeStartDelay, p => 2 / p.Recharge.Start.Value },
                { TotalOverride, _stat.EffectiveLeechRate, p => p.Leech.Rate.Value * p.RecoveryRate.Value },
                {
                    TotalOverride, _stat.AbsoluteLeechRate,
                    p => _stat.LeechTargetPoolValue(p) * _stat.EffectiveLeechRate(p).Value / 100
                },
                {
                    TotalOverride, _stat.AbsoluteLeechRateLimit,
                    p => _stat.LeechTargetPoolValue(p.BuildPool()) * p.Leech.RateLimit.Value / 100
                },
                {
                    TotalOverride, _stat.TimeToReachLeechRateLimit,
                    p => p.Leech.RateLimit.Value / p.Leech.Rate.Value / _stat.CastRate.Value
                },
                // flasks
                { PercentMore, Flask.LifeRecovery, Flask.Effect.Value * 100 },
                { PercentMore, Flask.ManaRecovery, Flask.Effect.Value * 100 },
                { PercentMore, Flask.LifeRecovery, Flask.RecoverySpeed.Value * 100 },
                { PercentMore, Flask.ManaRecovery, Flask.RecoverySpeed.Value * 100 },
                { PercentMore, Flask.Duration, (100 / Flask.RecoverySpeed.Value) - 100 },
                // ailments
                {
                    TotalOverride, _stat.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Ignite),
                    (int) DamageType.Fire
                },
                {
                    TotalOverride, _stat.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Bleed),
                    (int) DamageType.Physical
                },
                {
                    TotalOverride, _stat.AilmentDealtDamageType(Common.Builders.Effects.Ailment.Ignite),
                    (int) DamageType.Chaos
                },
                {
                    TotalOverride, _stat.AilmentCombinedEffectiveChance,
                    ailment => CombineSource(_stat.AilmentEffectiveChance(ailment), CombineHandsByAverage) *
                               (1 - Ailment.From(ailment).Avoidance.For(Enemy).Value / 100)
                },
                {
                    TotalOverride, _stat.AilmentEffectiveChance,
                    _ => _stat.EffectiveCritChance,
                    (ailment, critChance)
                        => ValueFactory.If(Ailment.From(ailment).CriticalStrikesAlwaysInflict.IsSet)
                            .Then(Ailment.From(ailment).Chance.Value / 100 * (1 - critChance.Value) + critChance.Value)
                            .Else(Ailment.From(ailment).Chance.Value)
                },
                {
                    TotalOverride, _stat.AilmentChanceWithCrits, _stat.AilmentChanceWithCrits,
                    (ailment, _) => ValueFactory
                        .If(Ailment.From(ailment).CriticalStrikesAlwaysInflict.IsSet).Then(100)
                        .Else(Ailment.From(ailment).Chance.Value)
                },
                // stun (see https://pathofexile.gamepedia.com/Stun)
                { PercentLess, Effect.Stun.Duration, Effect.Stun.Recovery.For(Enemy).Value * 100 },
                {
                    TotalOverride, _stat.EffectiveStunThreshold,
                    Effect.Stun.Threshold, EffectiveStunThresholdValue
                },
                {
                    BaseSet, Effect.Stun.Chance,
                    _stat.AverageDamage.WithHits, _stat.EffectiveStunThreshold,
                    (damage, threshold)
                        => 200 * damage.Value / (Life.For(Enemy).ValueFor(NodeType.Subtotal) * threshold.Value)
                },
                {
                    TotalOverride, _stat.StunAvoidanceWhileCasting,
                    1 -
                    (1 - Effect.Stun.Avoidance.Value) * (1 - Effect.Stun.ChanceToAvoidInterruptionWhileCasting.Value)
                },
            }.Concat(CreateAilmentSourceDamageTypeModifiers());

        private ValueBuilder ChanceToHitValue(
            IStatBuilder accuracyStat, IStatBuilder evasionStat, IConditionBuilder isBlinded)
        {
            var accuracy = accuracyStat.Value;
            var evasion = evasionStat.Value;
            var blindMultiplier = ValueFactory.If(isBlinded).Then(0.5).Else(1);
            return 100 * blindMultiplier * accuracy /
                   (accuracy + (evasion / 4).Select(d => Math.Pow(d, 0.8), v => $"{v}^0.8"));
        }

        private static ValueBuilder FailureProbability(IStatBuilder percentageChanceStat)
            => 1 - percentageChanceStat.Value / 100;

        private IEnumerable<IIntermediateModifier> CreateAilmentSourceDamageTypeModifiers()
        {
            foreach (var ailment in Enums.GetValues<Ailment>())
            {
                var ailmentBuilder = Ailment.From(ailment);
                foreach (var damageType in Enums.GetValues<DamageType>())
                {
                    var damageTypeBuilder = DamageTypeBuilders.From(damageType);
                    var builder = _modifierBuilder
                        .WithForm(TotalOverride)
                        .WithStat(damageTypeBuilder.Damage.With(ailmentBuilder))
                        .WithValue(ValueFactory.Create(0))
                        .WithCondition(ailmentBuilder.Source(damageTypeBuilder).IsSet.Not);
                    yield return builder.Build();
                }
            }
        }

        private IValueBuilder EffectiveStunThresholdValue(IStatBuilder stunThresholdStat)
        {
            // If stun threshold is less than 25%, it is scaled up.
            // See https://pathofexile.gamepedia.com/Stun#Stun_threshold
            var stunThreshold = stunThresholdStat.Value;
            return ValueFactory
                .If(stunThreshold >= 0.25).Then(stunThreshold)
                .Else(0.25 - 0.25 * (0.25 - stunThreshold) / (0.5 - stunThreshold));
        }

        private ValueBuilder CombineSource(
            IDamageRelatedStatBuilder statToCombine, Func<IDamageRelatedStatBuilder, IValueBuilder> handCombiner)
            => ValueFactory.If(_stat.SkillHitDamageSource.Value.Eq((int) DamageSource.Attack))
                .Then(handCombiner(statToCombine))
                .ElseIf(_stat.SkillHitDamageSource.Value.Eq((int) DamageSource.Spell))
                .Then(statToCombine.With(DamageSource.Spell).Value)
                .ElseIf(_stat.SkillHitDamageSource.Value.Eq((int) DamageSource.Secondary))
                .Then(statToCombine.With(DamageSource.Secondary).Value)
                .Else(0);

        private static ValueBuilder CombineHandsByAverage(IDamageRelatedStatBuilder statToCombine)
            => (statToCombine.With(AttackDamageHand.MainHand).Value +
                statToCombine.With(AttackDamageHand.OffHand).Value) / 2;

        private static Func<IDamageRelatedStatBuilder, ValueBuilder> CombineHandsByWeightedAverage(
            params IDamageRelatedStatBuilder[] weights)
        {
            var mhWeight = weights.Select(w => w.With(AttackDamageHand.MainHand).Value).Aggregate((l, r) => l * r);
            var ohWeight = weights.Select(w => w.With(AttackDamageHand.OffHand).Value).Aggregate((l, r) => l * r);
            return statToCombine => CombineByWeightedAverage(
                statToCombine.With(AttackDamageHand.MainHand).Value, mhWeight,
                statToCombine.With(AttackDamageHand.OffHand).Value, ohWeight);
        }

        private static ValueBuilder CombineByWeightedAverage(
            ValueBuilder left, ValueBuilder leftWeight, ValueBuilder right, ValueBuilder rightWeight)
            => (left * leftWeight + right * rightWeight) / (leftWeight + rightWeight);
    }
}