using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Equipment;

namespace PoESkillTree.Computation.Builders.Equipment
{
    public class ItemSlotBuilders : IItemSlotBuilders
    {
        public IItemSlotBuilder From(ItemSlot slot) => new Builder(slot);

        private class Builder : ConstantBuilder<IItemSlotBuilder, ItemSlot>, IItemSlotBuilder
        {
            public Builder(ItemSlot slot) : base(slot)
            {
            }
        }
    }
}