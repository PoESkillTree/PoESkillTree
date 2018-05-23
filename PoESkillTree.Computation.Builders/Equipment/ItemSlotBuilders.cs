using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Equipment
{
    public class ItemSlotBuilders : IItemSlotBuilders
    {
        public IItemSlotBuilder From(ItemSlot slot) => new Builder(slot);


        private class Builder : IItemSlotBuilder
        {
            private readonly ItemSlot _slot;

            public Builder(ItemSlot slot) => _slot = slot;

            public IItemSlotBuilder Resolve(ResolveContext context) => this;

            public ItemSlot Build() => _slot;
        }
    }
}