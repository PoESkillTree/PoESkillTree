using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class ItemParser : IParser<ItemParserParameter>
    {
        private readonly BaseItemDefinitions _baseItemDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;

        public ItemParser(
            BaseItemDefinitions baseItemDefinitions, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_coreParser, _builderFactories, _baseItemDefinitions) =
                (coreParser, builderFactories, baseItemDefinitions);

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var (item, slot) = parameter;
            var modifiers = new ModifierCollection(_builderFactories, new ModifierSource.Local.Item(slot));
            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var baseItemDefinition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);

            var equipmentBuilder = _builderFactories.EquipmentBuilders.Equipment[slot];
            modifiers.AddGlobal(equipmentBuilder.ItemTags, Form.BaseSet, baseItemDefinition.Tags.EncodeAsDouble());
            modifiers.AddGlobal(equipmentBuilder.ItemClass, Form.BaseSet, (double) baseItemDefinition.ItemClass);
            modifiers.AddGlobal(equipmentBuilder.FrameType, Form.BaseSet, (double) item.FrameType);
            if (item.IsCorrupted)
            {
                modifiers.AddGlobal(equipmentBuilder.Corrupted, Form.BaseSet, 1);
            }

            var parseResult = ParseResult.Success(modifiers.ToList());
            var coreParseResult = item.Modifiers.Values.Flatten().Select(s => Parse(s, globalSource));
            return ParseResult.Aggregate(parseResult.Concat(coreParseResult));
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }

    public class ItemParserParameter
    {
        public ItemParserParameter(Item item, ItemSlot itemSlot)
            => (Item, ItemSlot) = (item, itemSlot);

        public void Deconstruct(out Item item, out ItemSlot itemSlot)
            => (item, itemSlot) = (Item, ItemSlot);

        public Item Item { get; }
        public ItemSlot ItemSlot { get; }
    }
}