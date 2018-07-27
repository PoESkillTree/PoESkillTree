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

        IDamageRelatedStatBuilder AverageEffectiveDamage { get; }
        IStatBuilder AilmentDealtDamageType(Ailment ailment);

        IStatBuilder HitsPerSecond { get; }

        IDamageRelatedStatBuilder EffectiveStunThreshold { get; }
        IStatBuilder StunAvoidanceWhileCasting { get; }
    }
}