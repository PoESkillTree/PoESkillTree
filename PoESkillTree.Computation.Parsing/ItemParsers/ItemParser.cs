using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
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
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        private List<Modifier> _parsedModifiers;

        public ItemParser(
            BaseItemDefinitions baseItemDefinitions, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_coreParser, _builderFactories, _baseItemDefinitions) =
                (coreParser, builderFactories, baseItemDefinitions);

        public ParseResult Parse(ItemParserParameter parameter)
        {
            _parsedModifiers = new List<Modifier>();
            var (item, slot) = parameter;
            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var baseItemDefinition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);

            var equipmentBuilder = _builderFactories.EquipmentBuilders.Equipment[slot];
            AddModifier(equipmentBuilder.ItemTags, Form.BaseSet, baseItemDefinition.Tags.EncodeAsDouble(),
                globalSource);
            AddModifier(equipmentBuilder.ItemClass, Form.BaseSet, (double) baseItemDefinition.ItemClass, globalSource);
            AddModifier(equipmentBuilder.FrameType, Form.BaseSet, (double) item.FrameType, globalSource);
            if (item.IsCorrupted)
            {
                AddModifier(equipmentBuilder.Corrupted, Form.BaseSet, 1, globalSource);
            }

            var parseResult = ParseResult.Success(_parsedModifiers);
            var coreParseResult = item.Modifiers.Values.Flatten().Select(s => Parse(s, globalSource));
            _parsedModifiers = null;
            return ParseResult.Aggregate(parseResult.Concat(coreParseResult));
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);

        private void AddModifier(
            IStatBuilder stat, Form form, double value, ModifierSource modifierSource)
        {
            var builder = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(_builderFactories.ValueBuilders.Create(value));
            var intermediateModifier = builder.Build();
            _parsedModifiers.AddRange(intermediateModifier.Build(modifierSource, Entity.Character));
        }
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