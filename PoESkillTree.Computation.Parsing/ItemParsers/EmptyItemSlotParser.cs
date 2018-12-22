using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.ItemParsers
{
    public class EmptyItemSlotParser : IParser<ItemSlot>
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public EmptyItemSlotParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        public ParseResult Parse(ItemSlot itemSlot)
        {
            if (itemSlot != ItemSlot.MainHand)
                return ParseResult.Success(new Modifier[0]);

            var localSource = new ModifierSource.Local.Item(itemSlot);
            var globalSource = new ModifierSource.Global(localSource);

            var builder = _modifierBuilder
                .WithStat(_builderFactories.EquipmentBuilders.Equipment[itemSlot].ItemClass)
                .WithForm(_builderFactories.FormBuilders.From(Form.BaseSet))
                .WithValue(_builderFactories.ValueBuilders.Create((double) ItemClass.Unarmed));
            var intermediateModifier = builder.Build();
            var modifiers = intermediateModifier.Build(globalSource, Entity.Character);
            return ParseResult.Success(modifiers);
        }
    }
}