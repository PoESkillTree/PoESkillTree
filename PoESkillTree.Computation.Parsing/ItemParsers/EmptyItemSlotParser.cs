using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class EmptyItemSlotParser : IParser<ItemSlot>
    {
        private readonly IBuilderFactories _builderFactories;

        public EmptyItemSlotParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        public ParseResult Parse(ItemSlot itemSlot)
        {
            if (itemSlot != ItemSlot.MainHand)
                return ParseResult.Success(new Modifier[0]);

            var modifiers = new ModifierCollection(_builderFactories, new ModifierSource.Local.Item(itemSlot));
            modifiers.AddGlobal(_builderFactories.EquipmentBuilders.Equipment[itemSlot].ItemClass,
                Form.BaseSet, (double) ItemClass.Unarmed);
            return ParseResult.Success(modifiers.ToList());
        }
    }
}