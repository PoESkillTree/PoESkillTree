using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class ItemParser : IParser<ItemParserParameter>
    {
        private readonly ICoreParser _coreParser;

        public ItemParser(ICoreParser coreParser)
            => _coreParser = coreParser;

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var item = parameter.Item;
            var localSource = new ModifierSource.Local.Item(parameter.ItemSlot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var modifiers = item.ImplicitModifiers
                .Concat(item.CorruptionModifiers)
                .Concat(item.EnchantmentModifiers)
                .Concat(item.ExplicitModifiers)
                .Concat(item.CraftedModifiers);
            var parseResults = modifiers.Select(s => Parse(s, globalSource));
            return ParseResult.Aggregate(parseResults);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }

    public class ItemParserParameter
    {
        public ItemParserParameter(Item item, ItemSlot itemSlot)
            => (Item, ItemSlot) = (item, itemSlot);

        public Item Item { get; }
        public ItemSlot ItemSlot { get; }
    }
}