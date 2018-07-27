using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Contains stat builders that do not partake in parsing but are relevant for calculations.
    /// </summary>
    public interface IMetaStatBuilders
    {
        /// <summary>
        /// The value of the pool that is the target pool of <see cref="regenSourcePool"/>'s regen.
        /// </summary>
        ValueBuilder RegenTargetPoolValue(IPoolStatBuilder regenSourcePool);

        IDamageRelatedStatBuilder AverageEffectiveDamage { get; }
        IStatBuilder AilmentDealtDamageType(Ailment ailment);

        IDamageRelatedStatBuilder EffectiveStunThreshold { get; }
        IStatBuilder StunAvoidanceWhileCasting { get; }
    }
}