using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Charges
{
    /// <summary>
    /// Represents a charge type, e.g. Endurance Charge.
    /// </summary>
    public interface IChargeTypeBuilder : IResolvable<IChargeTypeBuilder>
    {
        /// <summary>
        /// Gets a stat representing the active amount of this charge type.
        /// </summary>
        IStatBuilder Amount { get; }

        /// <summary>
        /// Gets a stat representing the duration charges of this type last.
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// Gets a stat representing the chance to gain a charge of this type (only makes sense in combination with
        /// an action condition, e.g. "on hit").
        /// </summary>
        IStatBuilder ChanceToGain { get; }

        /// <summary>
        /// Gets an action occurring when Self gains a charge of this type.
        /// </summary>
        IActionBuilder GainAction { get; }
    }
}