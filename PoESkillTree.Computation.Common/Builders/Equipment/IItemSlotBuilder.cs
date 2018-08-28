using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common.Builders.Equipment
{
    /// <summary>
    /// Represents an <see cref="ItemSlot"/>.
    /// </summary>
    /// <remarks>
    /// Necessary to allow referencing and resolving item slots.
    /// </remarks>
    public interface IItemSlotBuilder : IResolvable<IItemSlotBuilder>
    {
        ItemSlot Build();
    }
}