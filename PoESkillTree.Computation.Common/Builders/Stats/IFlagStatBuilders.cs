namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for flag stats.
    /// </summary>
    public interface IFlagStatBuilders
    {
        /// <summary>
        /// Gets a stat indicating whether the movement speed penalties from equipment (as hidden reduced movement
        /// speed mods) should be ignored.
        /// </summary>
        IStatBuilder IgnoreMovementSpeedPenalties { get; }

        IStatBuilder ShieldModifiersApplyToMinionsInstead { get; }

        IStatBuilder IgnoreHexproof { get; }
        IStatBuilder CriticalStrikeChanceIsLucky { get; }
        IStatBuilder FarShot { get; }

        IStatBuilder IncreasesToSourceApplyToTarget(IStatBuilder source, IStatBuilder target);

        IStatBuilder AlwaysMoving { get; }
        IStatBuilder AlwaysStationary { get; }

        IStatBuilder BrandAttachedToEnemy { get; }
        IStatBuilder BannerPlanted { get; }
    }
}