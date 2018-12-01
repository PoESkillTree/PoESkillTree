using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class ItemParser : IParser<ItemParserParameter>
    {
        public ParseResult Parse(ItemParserParameter parameter)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ItemParserParameter
    {
        public ItemParserParameter(Item item, ItemSlot itemSlot)
            => (Item, ItemSlot) = (item, itemSlot);

        public Item Item { get; }
        public ItemSlot ItemSlot { get; }
    }
}