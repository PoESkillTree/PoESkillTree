using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Represents a collection of equipment slots.
    /// </summary>
    public interface IEquipmentBuilderCollection : IBuilderCollection<IEquipmentBuilder>
    {
        /// <summary>
        /// Gets the equipment of the given slot.
        /// </summary>
        IEquipmentBuilder this[ItemSlot slot] { get; }
    }
}