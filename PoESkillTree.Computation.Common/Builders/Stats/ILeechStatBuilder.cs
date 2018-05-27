using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
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
        /// Returns a flag stat indicating whether this stat's leech value applies to the given pool.
        /// <para>The flag for the pool this stat is obtained from is activated by default. If this is activated
        /// for any other pool, this stat's leech applies to that pool instead.</para>
        /// </summary>
        IFlagStatBuilder AppliesTo(IPoolStatBuilder stat);

        /// <summary>
        /// Returns a leech object through damage done by Self can be additionally leeched to the given entity.
        /// The given entities normally other Leech properties (e.g. Rate) also apply to this leech.
        /// </summary>
        /// <remarks>
        /// E.g. Chieftain's "1% of Damage dealt by your Totems is Leeched to you as Life" leeches totem damage to the
        /// character.
        /// </remarks>
        ILeechStatBuilder To(IEntityBuilder entity);

        /// <summary>
        /// Returns a flag stat indicating whether all leech of this instance's pool is based on the given damage type
        /// instead of the damage types of damage stats passed to <see cref="Of"/>.
        /// </summary>
        IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType);
    }
}