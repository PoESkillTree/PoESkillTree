using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Contains stat builders that do not partake in parsing but are relevant for calculations.
    /// </summary>
    public interface IMetaStatBuilders
    {
        IStatBuilder EffectiveRegen(Pool pool);
        IStatBuilder EffectiveRecharge(Pool pool);
        IStatBuilder RechargeStartDelay(Pool pool);
        IStatBuilder EffectiveLeechRate(Pool pool);
        IStatBuilder AbsoluteLeechRate(Pool pool);
        IStatBuilder AbsoluteLeechRateLimit(Pool pool);
        IStatBuilder TimeToReachLeechRateLimit(Pool pool);

        /// <summary>
        /// The value of the pool that is the target pool of <see cref="sourcePool"/>'s regen.
        /// </summary>
        ValueBuilder RegenTargetPoolValue(Pool sourcePool);

        ValueBuilder LeechTargetPoolValue(Pool sourcePool);

        // Skill damage calculation
        IDamageRelatedStatBuilder EnemyResistanceAgainstNonCrits(DamageType damageType);
        IDamageRelatedStatBuilder EnemyResistanceAgainstCrits(DamageType damageType);
        IDamageRelatedStatBuilder EffectiveDamageMultiplierWithNonCrits(DamageType damageType);
        IDamageRelatedStatBuilder EffectiveDamageMultiplierWithCrits(DamageType damageType);
        IDamageRelatedStatBuilder DamageWithNonCrits(DamageType damageType);
        IDamageRelatedStatBuilder DamageWithCrits(DamageType damageType);
        IDamageRelatedStatBuilder DamageWithNonCrits();
        IDamageRelatedStatBuilder DamageWithCrits();
        IDamageRelatedStatBuilder AverageDamagePerHit { get; }
        IDamageRelatedStatBuilder AverageDamage { get; }
        IStatBuilder AverageDamageWithHits { get; }
        IStatBuilder SkillDpsWithHits { get; }
        IStatBuilder SkillDpsWithDoTs { get; }

        IStatBuilder CastRate { get; }
        IStatBuilder CastTime { get; }

        IStatBuilder AilmentDealtDamageType(Ailment ailment);
        IDamageRelatedStatBuilder EffectiveCritChance { get; }

        IStatBuilder ResistanceAgainstHits(DamageType damageType);
        IStatBuilder MitigationAgainstHits(DamageType damageType);
        IStatBuilder MitigationAgainstDoTs(DamageType damageType);
        IStatBuilder ChanceToAvoidMeleeAttacks { get; }
        IStatBuilder ChanceToAvoidProjectileAttacks { get; }
        IStatBuilder ChanceToAvoidSpells { get; }

        IDamageRelatedStatBuilder EffectiveStunThreshold { get; }
        IStatBuilder StunAvoidanceWhileCasting { get; }

        IStatBuilder SkillHitDamageSource { get; }
    }
}