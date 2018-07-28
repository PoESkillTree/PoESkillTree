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
                    CriticalStrike.Chance.With(AttackDamageHand.MainHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.MainHand).Value
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(AttackDamageHand.OffHand),
                    CriticalStrike.Chance.With(AttackDamageHand.OffHand).Value *
                    Stat.ChanceToHit.With(AttackDamageHand.OffHand).Value
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(DamageSource.Spell),
                    CriticalStrike.Chance.With(DamageSource.Spell).Value
                },
                {
                    TotalOverride, _stat.EffectiveCritChance.With(DamageSource.Secondary),
                    CriticalStrike.Chance.With(DamageSource.Secondary).Value
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
                // stun (see https://pathofexile.gamepedia.com/Stun)
                { PercentLess, Effect.Stun.Duration, Effect.Stun.Recovery.For(Enemy).Value * 100 },
                {
                    TotalOverride, _stat.EffectiveStunThreshold,
                    Effect.Stun.Threshold, EffectiveStunThresholdValue
                },
                {
                    BaseSet, Effect.Stun.Chance,
                    _stat.AverageDamage, _stat.EffectiveStunThreshold, StunChanceValue
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

        private IValueBuilder StunChanceValue(IStatBuilder damageStat, IStatBuilder stunThresholdStat)
            => 200 * damageStat.Value /
               (Life.For(Enemy).ValueFor(NodeType.Subtotal) * stunThresholdStat.Value);
    }
}