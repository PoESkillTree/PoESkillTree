namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Factory interface for equipment.
    /// </summary>
    public interface IEquipmentBuilders
    {
        /// <summary>
        /// Gets an equipment collection for all slots except flasks and jewels.
        /// </summary>
        IEquipmentBuilderCollection Equipment { get; }
    }
}