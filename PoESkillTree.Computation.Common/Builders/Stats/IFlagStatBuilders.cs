namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for flag stats.
    /// </summary>
    public interface IFlagStatBuilders
    {
        /// <summary>
        /// Gets a flag stat indicating whether the movement speed penalties from equipment (as hidden reduced movement
        /// speed mods) should be ignored.
        /// </summary>
        IFlagStatBuilder IgnoreMovementSpeedPenalties { get; }

        IFlagStatBuilder ShieldModifiersApplyToMinionsInstead { get; }
    }
}