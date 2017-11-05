using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Parsing.Builders.Equipment
{
    public interface IItemSlotBuilders
    {
        IItemSlotBuilder From(ItemSlot slot);
    }
}