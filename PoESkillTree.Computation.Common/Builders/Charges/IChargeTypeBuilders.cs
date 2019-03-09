using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Charges
{
    /// <summary>
    /// Factory interface for charge types.
    /// </summary>
    public interface IChargeTypeBuilders
    {
        /// <summary>
        /// Gets the charge type representing Endurance charges.
        /// </summary>
        IChargeTypeBuilder Endurance { get; }

        /// <summary>
        /// Gets the charge type representing Frenzy charges.
        /// </summary>
        IChargeTypeBuilder Frenzy { get; }

        /// <summary>
        /// Gets the charge type representing Power charges.
        /// </summary>
        IChargeTypeBuilder Power { get; }
        
        /// <summary>
        /// Gets a stat representing the chance to steal a charge (only makes sense in combination with
        /// an action condition, e.g. "on hit").
        /// </summary>
        IDamageRelatedStatBuilder ChanceToSteal { get; }

        IChargeTypeBuilder Rage { get; }
        IStatBuilder RageEffect { get; }

        IChargeTypeBuilder From(ChargeType type);
    }
}