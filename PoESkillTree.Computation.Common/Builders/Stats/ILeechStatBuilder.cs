using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents the leech stats related to a pool.
    /// </summary>
    public interface ILeechStatBuilder
    {
        /// <summary>
        /// Returns a stat representing the percentage of damage done matching <paramref name="damage"/> that leeched
        /// to the pool this instance applies to.
        /// </summary>
        IStatBuilder Of(IDamageRelatedStatBuilder damage);

        /// <summary>
        /// Gets a stat representing the percentage of this instance's pool that can be leeched per second at most
        /// (over all active leech instances).
        /// </summary>
        IStatBuilder RateLimit { get; }
        /// <summary>
        /// Gets a stat representing the percentage of this instance's pool that is leeched by a single leech instance
        /// per second.
        /// </summary>
        IStatBuilder Rate { get; }

        /// <summary>
        /// Gets a stat representing the percentage of this instance's pool a single leech instance can recover at
        /// maximum (before being increased by Rate).
        /// </summary>
        IStatBuilder MaximumRecoveryPerInstance { get; }

        /// <summary>
        /// True if the entity is currently leeching to this instance's pool.
        /// </summary>
        IConditionBuilder IsActive { get; }

        /// <summary>
        /// Gets a stat representing whether leech applied to this instance's pool is applied instantly.
        /// </summary>
        IStatBuilder IsInstant { get; }
    }
}