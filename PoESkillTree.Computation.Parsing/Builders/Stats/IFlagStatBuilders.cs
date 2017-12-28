namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Factory interface for flag stats.
    /// </summary>
    public interface IFlagStatBuilders
    {
        /// <summary>
        /// Gets a flag stat indicating whether Onslaugt's stats should be applied to Self.
        /// </summary>
        IFlagStatBuilder Onslaught { get; }

        /// <summary>
        /// Gets a flag stat indicating whether Unholy Might's stats should be applied to Self.
        /// </summary>
        IFlagStatBuilder UnholyMight { get; }

        /// <summary>
        /// Gets a flag stat indicating whether Phasing's stats should be applied to Self.
        /// </summary>
        IFlagStatBuilder Phasing { get; }

        /// <summary>
        /// Gets a flag stat indicating whether the movement speed penalties from equipment (as hidden reduced movement
        /// speed mods) should be ignored.
        /// </summary>
        IFlagStatBuilder IgnoreMovementSpeedPenalties { get; }
    }
}