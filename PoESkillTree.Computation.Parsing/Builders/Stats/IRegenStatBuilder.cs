namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Represent the regeneration stat of a pool. Its value is the amount of a pool regenerated per second.
    /// </summary>
    public interface IRegenStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Gets a stat representing the percentage of the pool's value that is regenerated per second. The returned
        /// stat's value (as percentage of the pool's value) will be added to the regen stat's value.
        /// </summary>
        IStatBuilder Percent { get; }

        /// <summary>
        /// Returns a flag stat indicating whether this stat's regeneration value applies to the given pool.
        /// <para>The flag for the pool this stat is obtained from is activated by default. If this is activated
        /// for any other pool, this stat's regeneration applies to that pool instead.</para>
        /// </summary>
        IFlagStatBuilder AppliesTo(IPoolStatBuilder stat);
    }
}