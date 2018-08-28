using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Factory interface for item slots.
    /// </summary>
    public interface IItemSlotBuilders
    {
        /// <summary>
        /// Returns the builder representation of the given <see cref="ItemSlot"/>.
        /// </summary>
        IItemSlotBuilder From(ItemSlot slot);
    }
}