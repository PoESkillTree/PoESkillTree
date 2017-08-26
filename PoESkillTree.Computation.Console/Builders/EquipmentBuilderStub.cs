using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EquipmentBuilderStub : BuilderStub, IEquipmentBuilder
    {
        private readonly IConditionBuilders _conditionBuilders;

        public EquipmentBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IConditionBuilder Has(Tags tag) =>
            new ConditionBuilderStub($"{this} has tag {tag}");

        public IConditionBuilder Has(FrameType frameType) =>
            new ConditionBuilderStub($"{this} has frame type {frameType}");

        public IConditionBuilder HasItem => new ConditionBuilderStub($"{this} has item");
        public IConditionBuilder IsCorrupted => new ConditionBuilderStub($"{this} is corrupted");

        public IFlagStatBuilder AppliesToSelf => 
            new FlagStatBuilderStub($"{this} applies to self", _conditionBuilders);

        public IFlagStatBuilder AppliesToMinions => 
            new FlagStatBuilderStub($"{this} applies to minions", _conditionBuilders);
    }


    public class EquipmentBuilderCollectionStub : BuilderCollectionStub<IEquipmentBuilder>, 
        IEquipmentBuilderCollection
    {
        private readonly IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> _elements;

        public EquipmentBuilderCollectionStub(
            IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> elements,
            IConditionBuilders conditionBuilders) 
            : base(elements.Values.ToList(), conditionBuilders)
        {
            _elements = elements;
        }

        public IEquipmentBuilder this[ItemSlot slot] => _elements[slot];
    }


    public class EquipmentBuildersStub : IEquipmentBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public EquipmentBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
            var elements = new Dictionary<ItemSlot, IEquipmentBuilder>();
            foreach (var itemSlot in typeof(ItemSlot).GetEnumValues().Cast<ItemSlot>())
            {
                elements[itemSlot] =
                    new EquipmentBuilderStub(itemSlot.ToString(), conditionBuilders);
            }
            Equipment = new EquipmentBuilderCollectionStub(elements, conditionBuilders);
        }

        public IEquipmentBuilderCollection Equipment { get; }

        public IEquipmentBuilder LocalHand =>
            new EquipmentBuilderStub("Local Hand", _conditionBuilders);
    }
}