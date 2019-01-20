using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    /// <summary>
    /// Partial parser of <see cref="ItemParser"/> that parses equipment related modifiers
    /// </summary>
    public class ItemEquipmentParser : IParser<PartialItemParserParameter>
    {
        private readonly IBuilderFactories _builderFactories;

        public ItemEquipmentParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        public ParseResult Parse(PartialItemParserParameter parameter)
        {
            var (item, slot, baseItemDefinition, localSource, _) = parameter;
            var modifiers = new ModifierCollection(_builderFactories, localSource);
            var equipmentBuilder = _builderFactories.EquipmentBuilders.Equipment[slot];

            modifiers.AddGlobal(equipmentBuilder.ItemTags, Form.TotalOverride, baseItemDefinition.Tags.EncodeAsDouble());
            modifiers.AddGlobal(equipmentBuilder.ItemClass, Form.TotalOverride, (double) baseItemDefinition.ItemClass);
            modifiers.AddGlobal(equipmentBuilder.FrameType, Form.TotalOverride, (double) item.FrameType);
            if (item.IsCorrupted)
            {
                modifiers.AddGlobal(equipmentBuilder.Corrupted, Form.TotalOverride, 1);
            }

            return ParseResult.Success(modifiers.ToList());
        }
    }
}