using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents the leech stats related to a pool.
    /// </summary>
    public interface ILeechStatBuilder : IResolvable<ILeechStatBuilder>
    {
        /// <summary>
        /// Returns a stat representing the percentage of damage done matching <paramref name="damage"/> that leeched
        /// to the pool this instance applies to.
        /// </summary>
        IStatBuilder Of(IDamageStatBuilder damage);

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
        /// Returns a flag stat indicating whether this stat's leech value applies to the given pool instead.
        /// </summary>
        IFlagStatBuilder AppliesToInstead(Pool pool);

        /// <summary>
        /// Returns a flag stat indicating whether all leech of this instance's pool is based on the given damage type
        /// instead of the damage types of damage stats passed to <see cref="Of"/>.
        /// </summary>
        IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType);
    }
}