using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
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
        /// Gets a stat representing the modififer to the duration of flasks.
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// Gets a stat representing the modififer to the life recovered by flasks.
        /// </summary>
        IStatBuilder LifeRecovery { get; }

        /// <summary>
        /// Gets a stat representing the modififer to the mana recovered by flasks.
        /// </summary>
        IStatBuilder ManaRecovery { get; }

        /// <summary>
        /// Gets a stat representing the modififer to the life and mana recovery speed of flasks.
        /// </summary>
        IStatBuilder RecoverySpeed { get; }

        /// <summary>
        /// Gets a stat representing the modififer to the charges used by activating a flask.
        /// </summary>
        IStatBuilder ChargesUsed { get; }

        /// <summary>
        /// Gets a stat representing the modififer to flask charges gained.
        /// </summary>
        IStatBuilder ChargesGained { get; }

        /// <summary>
        /// Gets a condition that is satisfied if any flask is currently active.
        /// </summary>
        IConditionBuilder IsAnyActive { get; }
    }
}