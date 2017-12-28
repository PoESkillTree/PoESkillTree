using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    /// <summary>
    /// Represents a collection of equipment slots.
    /// </summary>
    public interface IEquipmentBuilderCollection : IBuilderCollection<IEquipmentBuilder>
    {
        /// <summary>
        /// Gets the given slot.
        /// </summary>
        IEquipmentBuilder this[ItemSlot slot] { get; }
    }
}