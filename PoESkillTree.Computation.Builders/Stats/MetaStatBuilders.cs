using System;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class MetaStatBuilders : StatBuildersBase, IMetaStatBuilders
    {
        public MetaStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public ValueBuilder RegenTargetPoolValue(Pool sourcePool) =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => BuildTargetPoolValue(ps, StatFactory.RegenTargetPool(ps.ModifierSourceEntity, sourcePool)),
                _ => RegenTargetPoolValue(sourcePool)));

        public ValueBuilder LeechTargetPoolValue(Pool sourcePool) =>
            new ValueBuilder(new ValueBuilderImpl(
                ps => BuildTargetPoolValue(ps, StatFactory.LeechTargetPool(ps.ModifierSourceEntity, sourcePool)),
                _ => LeechTargetPoolValue(sourcePool)));

        private IValue BuildTargetPoolValue(BuildParameters parameters, IStat targetPoolStat)
        {
            var entity = parameters.ModifierSourceEntity;
            var targetPoolValue = new StatValue(targetPoolStat);
            return new FunctionalValue(
                c => c.GetValue(TargetPoolValueStat(targetPoolValue.Calculate(c))),
                $"Value of Pool {targetPoolValue}");

            IStat TargetPoolValueStat(NodeValue? targetPool)
            {
                var targetPoolString = ((Pool) targetPool.Single()).ToString();
                return StatFactory.FromIdentity(targetPoolString, entity, typeof(int));
            }
        }

        public IStatBuilder EffectiveRegen(Pool pool) => FromIdentity($"{pool}.EffectiveRegen", typeof(int));
        public IStatBuilder EffectiveRecharge(Pool pool) => FromIdentity($"{pool}.EffectiveRecharge", typeof(int));
        public IStatBuilder RechargeStartDelay(Pool pool) => FromIdentity($"{pool}.RechargeStartDelay", typeof(double));

        public IStatBuilder EffectiveLeechRate(Pool pool) => FromIdentity($"{pool}.Leech.EffectiveRate", typeof(int));

        public IStatBuilder AbsoluteLeechRate(Pool pool) => FromIdentity($"{pool}.Leech.AbsoluteRate", typeof(double));

        public IStatBuilder AbsoluteLeechRateLimit(Pool pool)
            => FromIdentity($"{pool}.Leech.AbsoluteRateLimit", typeof(double));

        public IStatBuilder TimeToReachLeechRateLimit(Pool pool)
            => FromIdentity($"{pool}.Leech.SecondsToReachRateLimit", typeof(double));


        public IDamageRelatedStatBuilder Damage(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.Damage", typeof(double));

        public IDamageRelatedStatBuilder EnemyResistanceAgainstNonCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.EnemyResistance.NonCrits", typeof(int)).WithHits;

        public IDamageRelatedStatBuilder EnemyResistanceAgainstCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.EnemyResistance.Crits", typeof(int)).WithHits;

        public IDamageRelatedStatBuilder EffectiveDamageMultiplierWithNonCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.EffectiveDamageMultiplier.NonCrits", typeof(double));

        public IDamageRelatedStatBuilder EffectiveDamageMultiplierWithCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.EffectiveDamageMultiplier.Crits", typeof(double))
                .WithHitsAndAilments;

        public IDamageRelatedStatBuilder DamageWithNonCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.Damage.NonCrits", typeof(int));

        public IDamageRelatedStatBuilder DamageWithCrits(DamageType damageType)
            => DamageRelatedFromIdentity($"{damageType}.Damage.Crits", typeof(int));

        public IDamageRelatedStatBuilder DamageWithNonCrits()
            => DamageRelatedFromIdentity("Damage.NonCrits", typeof(int));

        public IDamageRelatedStatBuilder DamageWithCrits()
            => DamageRelatedFromIdentity("Damage.Crits", typeof(int));

        public IDamageRelatedStatBuilder AverageDamagePerHit
            => DamageRelatedFromIdentity(typeof(double)).WithHits;

        public IDamageRelatedStatBuilder AverageDamage => DamageRelatedFromIdentity(typeof(double));
        public IStatBuilder AverageHitDamage => FromIdentity("AverageDamage.Hit", typeof(double));
        public IStatBuilder SkillDpsWithHits => FromIdentity("DPS.Hit", typeof(double));
        public IStatBuilder SkillDpsWithDoTs => FromIdentity("DPS.OverTime", typeof(double));

        public IStatBuilder AverageAilmentDamage(Ailment ailment)
            => FromIdentity($"AverageDamage.{ailment}", typeof(double));

        public IStatBuilder AilmentInstanceLifetimeDamage(Ailment ailment)
            => FromIdentity($"InstanceLifetimeDamage.{ailment}", typeof(double));

        public IStatBuilder AilmentDps(Ailment ailment)
            => FromIdentity($"DPS.{ailment}", typeof(double));


        public IStatBuilder CastRate => FromIdentity(typeof(double));
        public IStatBuilder CastTime => FromIdentity(typeof(double));

        public IStatBuilder AilmentDealtDamageType(Ailment ailment)
            => FromFactory(e => StatFactory.AilmentDealtDamageType(e, ailment));

        public IDamageRelatedStatBuilder AilmentChanceWithCrits(Ailment ailment)
            => DamageRelatedFromIdentity($"{ailment}.ChanceWithCrits", typeof(double)).WithHits;

        public IDamageRelatedStatBuilder AilmentEffectiveChance(Ailment ailment)
            => DamageRelatedFromIdentity($"{ailment}.EffectiveChance", typeof(double)).WithHits;

        public IStatBuilder AilmentCombinedEffectiveChance(Ailment ailment)
            => FromIdentity($"{ailment}.EffectiveChance", typeof(double));

        public IStatBuilder AilmentEffectiveInstances(Ailment ailment)
            => FromIdentity($"{ailment}.EffectiveInstances", typeof(double));

        public IStatBuilder IncreasedDamageTakenFromShocks
            => FromIdentity("Shock.IncreasedDamageTaken", typeof(int), ExplicitRegistrationTypes.UserSpecifiedValue());

        public IStatBuilder ReducedActionSpeedFromChill
            => FromIdentity("Chill.ReducedAnimationSpeed", typeof(int), ExplicitRegistrationTypes.UserSpecifiedValue());

        public IDamageRelatedStatBuilder EffectiveCritChance
            => DamageRelatedFromIdentity("CriticalStrike.EffectiveChance", typeof(double)).WithHits;


        public IStatBuilder ResistanceAgainstHits(DamageType damageType)
            => FromIdentity($"{damageType}.ResistanceAgainstHits", typeof(int));

        public IStatBuilder MitigationAgainstHits(DamageType damageType)
            => FromIdentity($"{damageType}.MitigationAgainstHits", typeof(int));

        public IStatBuilder MitigationAgainstDoTs(DamageType damageType)
            => FromIdentity($"{damageType}.MitigationAgainstDoTs", typeof(int));

        public IStatBuilder ChanceToAvoidMeleeAttacks => FromIdentity(typeof(int));
        public IStatBuilder ChanceToAvoidProjectileAttacks => FromIdentity(typeof(int));
        public IStatBuilder ChanceToAvoidSpells => FromIdentity(typeof(int));

        public IDamageRelatedStatBuilder EffectiveStunThreshold
            => DamageRelatedFromIdentity("Stun.EffectiveThreshold", typeof(double)).WithHits;

        public IStatBuilder StunAvoidanceWhileCasting => FromIdentity("Stun.ChanceToAvoidWhileCasting", typeof(double));

        public IStatBuilder SkillHitDamageSource => FromIdentity(typeof(DamageSource));
        public IStatBuilder SkillUsesHand(AttackDamageHand hand) => FromIdentity($"SkillUses.{hand}", typeof(bool));
        public IStatBuilder SkillNumberOfHitsPerCast => FromIdentity(typeof(int));
        public IStatBuilder SkillDoubleHitsWhenDualWielding => FromIdentity(typeof(bool));

        public IStatBuilder MainSkillSocket(ItemSlot itemSlot, int socketIndex)
            => FromIdentity($"{itemSlot}.{socketIndex}.IsMainSkill", typeof(bool));

        public IStatBuilder MainSkillId => FromFactory(StatFactory.MainSkillId);

        public IStatBuilder MainSkillHasKeyword(Keyword keyword)
            => FromFactory(e => StatFactory.MainSkillHasKeyword(e, keyword));

        public IStatBuilder MainSkillPartHasKeyword(Keyword keyword)
            => FromFactory(e => StatFactory.MainSkillPartHasKeyword(e, keyword));

        public IStatBuilder MainSkillPartCastRateHasKeyword(Keyword keyword)
            => FromFactory(e => StatFactory.MainSkillPartCastRateHasKeyword(e, keyword));

        public IStatBuilder MainSkillPartDamageHasKeyword(Keyword keyword, DamageSource damageSource)
            => FromFactory(e => StatFactory.MainSkillPartDamageHasKeyword(e, keyword, damageSource));

        public IStatBuilder MainSkillPartAilmentDamageHasKeyword(Keyword keyword)
            => FromFactory(e => StatFactory.MainSkillPartAilmentDamageHasKeyword(e, keyword));

        public IStatBuilder ActiveSkillItemSlot(string skillId)
            => FromIdentity($"{skillId}.ActiveSkillItemSlot", typeof(ItemSlot));

        public IStatBuilder ActiveSkillSocketIndex(string skillId)
            => FromIdentity($"{skillId}.ActiveSkillSocketIndex", typeof(int));

        public IStatBuilder DamageBaseAddEffectiveness => FromFactory(StatFactory.DamageBaseAddEffectiveness);
        public IStatBuilder DamageBaseSetEffectiveness => FromFactory(StatFactory.DamageBaseSetEffectiveness);

        public IStatBuilder SelectedBandit => FromIdentity(typeof(Bandit));
        public IStatBuilder SelectedQuestPart => FromIdentity(typeof(QuestPart));

        private IStatBuilder FromFactory(Func<Entity, IStat> factory)
            => new StatBuilder(StatFactory, new LeafCoreStatBuilder(factory));
    }
}