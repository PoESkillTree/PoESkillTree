namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for flask stats.
    /// </summary>
    public interface IFlaskStatBuilders
    {
        /// <summary>
        /// Gets a stat representing the modifier to stats applied by flasks.
        /// </summary>
        IStatBuilder Effect { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the duration of flasks.
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the life recovered by flasks per second.
        /// </summary>
        IStatBuilder LifeRecovery { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the mana recovered by flasks per second.
        /// </summary>
        IStatBuilder ManaRecovery { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the life recovery speed of flasks.
        /// </summary>
        IStatBuilder LifeRecoverySpeed { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the mana recovery speed of flasks.
        /// </summary>
        IStatBuilder ManaRecoverySpeed { get; }

        /// <summary>
        /// Gets a stat representing the percentage of a flask's recovery that is applied instantly.
        /// </summary>
        IStatBuilder InstantRecovery { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the charges used by activating a flask.
        /// </summary>
        IStatBuilder ChargesUsed { get; }

        /// <summary>
        /// Gets a stat representing the modifier to flask charges gained.
        /// </summary>
        IStatBuilder ChargesGained { get; }

        /// <summary>
        /// Gets a stat representing the modifier to the maximum amount of flask charges.
        /// </summary>
        IStatBuilder MaximumCharges { get; }

        /// <summary>
        /// Gets the chance to gain a flask charge. Only useful as ExplicitRegistrationType.GainOnAction.
        /// </summary>
        IStatBuilder ChanceToGainCharge { get; }
    }
}