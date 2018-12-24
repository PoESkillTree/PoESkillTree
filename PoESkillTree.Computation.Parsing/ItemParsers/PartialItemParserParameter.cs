using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class PartialItemParserParameter
    {
        public PartialItemParserParameter(
            Item item, ItemSlot itemSlot, BaseItemDefinition baseItemDefinition,
            ModifierSource.Local localSource, ModifierSource.Global globalSource)
            => (Item, ItemSlot, BaseItemDefinition, LocalSource, GlobalSource) =
                (item, itemSlot, baseItemDefinition, localSource, globalSource);

        public void Deconstruct(
            out Item item, out ItemSlot itemSlot, out BaseItemDefinition baseItemDefinition,
            out ModifierSource.Local localSource, out ModifierSource.Global globalSource)
            => (item, itemSlot, baseItemDefinition, localSource, globalSource) =
                (Item, ItemSlot, BaseItemDefinition, LocalSource, GlobalSource);

        public Item Item { get; }
        public ItemSlot ItemSlot { get; }
        public BaseItemDefinition BaseItemDefinition { get; }

        public ModifierSource.Local LocalSource { get; }
        public ModifierSource.Global GlobalSource { get; }
    }
}