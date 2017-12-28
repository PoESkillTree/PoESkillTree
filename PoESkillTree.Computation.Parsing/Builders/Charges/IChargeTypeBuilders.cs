namespace PoESkillTree.Computation.Parsing.Builders.Charges
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
    }
}