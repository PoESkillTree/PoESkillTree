using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats related to projectiles.
    /// </summary>
    public interface IProjectileStatBuilders
    {
        /// <summary>
        /// Gets a stat representing the speed of projectiles.
        /// </summary>
        IStatBuilder Speed { get; }

        /// <summary>
        /// Gets a stat representing the amount of projectiles.
        /// </summary>
        IStatBuilder Count { get; }

        /// <summary>
        /// Gets a stat representing the amount targets pierced by projectiles.
        /// </summary>
        IStatBuilder PierceCount { get; }

        /// <summary>
        /// Gets a stat representing the number of times projectiles chain.
        /// </summary>
        IStatBuilder ChainCount { get; }

        /// <summary>
        /// Gets a stat representing whether projectiles fork.
        /// </summary>
        IStatBuilder Fork { get; }

        /// <summary>
        /// Gets the distance traveled by projectiles.
        /// </summary>
        ValueBuilder TravelDistance { get; }
    }
}