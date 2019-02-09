using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    /// <summary>
    /// Parser for items in ItemSlots
    /// </summary>
    public class ItemParser : IParser<ItemParserParameter>
    {
        private readonly BaseItemDefinitions _baseItemDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;
        private readonly IStatTranslator _statTranslator;

        public ItemParser(
            BaseItemDefinitions baseItemDefinitions, IBuilderFactories builderFactories, ICoreParser coreParser,
            IStatTranslator statTranslator)
            => (_baseItemDefinitions, _builderFactories, _coreParser, _statTranslator) = (baseItemDefinitions,
                builderFactories, coreParser, statTranslator);

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var (item, slot) = parameter;

            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var baseItemDefinition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);
            var partialParserParameter =
                new PartialItemParserParameter(item, slot, baseItemDefinition, localSource, globalSource);

            var parseResults = new List<ParseResult>();
            foreach (var partialParser in CreatePartialParsers())
            {
                parseResults.Add(partialParser.Parse(partialParserParameter));
            }
            return ParseResult.Aggregate(parseResults);
        }

        private IEnumerable<IParser<PartialItemParserParameter>> CreatePartialParsers()
            => new IParser<PartialItemParserParameter>[]
            {
                new ItemEquipmentParser(_builderFactories),
                new ItemPropertyParser(_builderFactories),
                new ItemModifierParser(_builderFactories, _coreParser, _statTranslator),
            };
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