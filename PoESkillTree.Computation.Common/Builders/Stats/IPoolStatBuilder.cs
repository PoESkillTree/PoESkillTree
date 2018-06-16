using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents stats for a <see cref="Pool"/> that have values, and support things like
    /// regeneration, gain and leech.
    /// </summary>
    /// <remarks>
    /// Values of these stats represent the maximum values of the pool, not the current, because calculation doesn't
    /// really need the concept of current life/mana/es beyond simple conditions like full or low.
    /// </remarks>
    public interface IPoolStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Applies this stat to <paramref name="entity"/> instead of the currently modified entity.
        /// See <see cref="IConditionBuilders.For"/> for more information.
        /// </summary>
        new IPoolStatBuilder For(IEntityBuilder entity);

        /// <summary>
        /// Gets a stat representing the regeneration value of this pool.
        /// </summary>
        IRegenStatBuilder Regen { get; }

        /// <summary>
        /// Gets a stat representing the recharge value of this pool.
        /// </summary>
        IRechargeStatBuilder Recharge { get; }

        /// <summary>
        /// Gets a stat representing the recovery rate of this pool, which is a modifier applying to regeneration,
        /// recharge and leech.
        /// </summary>
        IStatBuilder RecoveryRate { get; }

        /// <summary>
        /// Gets a stat representing the amount of this pool spent when casting the main skill once.
        /// </summary>
        IStatBuilder Cost { get; }

        /// <summary>
        /// Gets a stat representing the percentage of this pool that is reserved.
        /// </summary>
        IStatBuilder Reservation { get; }

        /// <summary>
        /// Gets an object representing the leech stats of this pool.
        /// </summary>
        ILeechStatBuilder Leech { get; }

        /// <summary>
        /// Gets a stat representing whether leech applied to this pool is applied instantly.
        /// </summary>
        /// <remarks>
        /// Not in ILeechStatBuilder because it does not convert with .AppliesTo(), see Bloodseeker and legacy
        /// Atziri's Acuity.
        /// </remarks>
        IFlagStatBuilder InstantLeech { get; }

        /// <summary>
        /// Gets a stat representing the flat gains applied to this stat. Requires an action condition to make sense,
        /// e.g. on kill or on hit.
        /// </summary>
        IStatBuilder Gain { get; }

        /// <summary>
        /// Gets a condition that is satisfied if this pool's current value is equal to 
        /// <see cref="IStatBuilder.Value"/>. It is satisfied if <c>Reservation.Value == 0</c> is satisfied and the
        /// user says this pool is full.
        /// </summary>
        IConditionBuilder IsFull { get; }

        /// <summary>
        /// Gets a condition that is satisfied if this pool's current value lower than or equal to 35% of 
        /// <see cref="IStatBuilder.Value"/>. It is satisfied if <c>Reservation.Value >= 65</c> is satisfied or the
        /// user says this pool is low.
        /// </summary>
        IConditionBuilder IsLow { get; }
    }
}