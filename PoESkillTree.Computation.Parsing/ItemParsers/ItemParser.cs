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

        private ModifierCollection _modifiers;

        public ItemParser(
            BaseItemDefinitions baseItemDefinitions, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_coreParser, _builderFactories, _baseItemDefinitions) =
                (coreParser, builderFactories, baseItemDefinitions);

        public ParseResult Parse(ItemParserParameter parameter)
        {
            var (item, slot) = parameter;
            _modifiers = new ModifierCollection(_builderFactories, new ModifierSource.Local.Item(slot));
            var localSource = new ModifierSource.Local.Item(slot, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var baseItemDefinition = _baseItemDefinitions.GetBaseItemById(item.BaseMetadataId);

            AddEquipmentModifiers(item, slot, baseItemDefinition);
            AddRequirementModifiers(item, baseItemDefinition);

            var parseResult = ParseResult.Success(_modifiers.ToList());
            var coreParseResult = item.Modifiers.Values.Flatten().Select(s => Parse(s, globalSource));
            return ParseResult.Aggregate(parseResult.Concat(coreParseResult));
        }

        private void AddEquipmentModifiers(Item item, ItemSlot slot, BaseItemDefinition baseItemDefinition)
        {
            var equipmentBuilder = _builderFactories.EquipmentBuilders.Equipment[slot];
            _modifiers.AddGlobal(equipmentBuilder.ItemTags, Form.BaseSet, baseItemDefinition.Tags.EncodeAsDouble());
            _modifiers.AddGlobal(equipmentBuilder.ItemClass, Form.BaseSet, (double) baseItemDefinition.ItemClass);
            _modifiers.AddGlobal(equipmentBuilder.FrameType, Form.BaseSet, (double) item.FrameType);
            if (item.IsCorrupted)
            {
                _modifiers.AddGlobal(equipmentBuilder.Corrupted, Form.BaseSet, 1);
            }
        }

        private void AddRequirementModifiers(Item item, BaseItemDefinition baseItemDefinition)
        {
            var requirementStats = _builderFactories.StatBuilders.Requirements;
            var requirements = baseItemDefinition.Requirements;
            _modifiers.AddLocal(requirementStats.Level, Form.BaseSet, item.RequiredLevel);
            if (requirements.Dexterity > 0)
            {
                _modifiers.AddLocal(requirementStats.Dexterity, Form.BaseSet, requirements.Dexterity);
            }
            if (requirements.Intelligence > 0)
            {
                _modifiers.AddLocal(requirementStats.Intelligence, Form.BaseSet, requirements.Intelligence);
            }
            if (requirements.Strength > 0)
            {
                _modifiers.AddLocal(requirementStats.Strength, Form.BaseSet, requirements.Strength);
            }
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