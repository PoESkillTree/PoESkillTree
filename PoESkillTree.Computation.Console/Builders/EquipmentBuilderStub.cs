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
        public EquipmentBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IConditionBuilder Has(Tags tag) =>
            new ConditionBuilderStub($"{this} has tag {tag}");

        public IConditionBuilder Has(FrameType frameType) =>
            new ConditionBuilderStub($"{this} has frame type {frameType}");

        public IConditionBuilder HasItem => new ConditionBuilderStub($"{this} has item");
        public IConditionBuilder IsCorrupted => new ConditionBuilderStub($"{this} is corrupted");

        public IFlagStatBuilder AppliesToSelf => 
            new FlagStatBuilderStub($"{this} applies to self");

        public IFlagStatBuilder AppliesToMinions => 
            new FlagStatBuilderStub($"{this} applies to minions");
    }


    public class EquipmentBuilderCollectionStub : BuilderCollectionStub<IEquipmentBuilder>, 
        IEquipmentBuilderCollection
    {
        private readonly IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> _elements;

        public EquipmentBuilderCollectionStub(
            IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> elements) 
            : base(elements.Values.ToList())
        {
            _elements = elements;
        }

        public IEquipmentBuilder this[ItemSlot slot] => _elements[slot];
    }


    public class EquipmentBuildersStub : IEquipmentBuilders
    {
        public EquipmentBuildersStub()
        {
            var elements = new Dictionary<ItemSlot, IEquipmentBuilder>();
            foreach (var itemSlot in typeof(ItemSlot).GetEnumValues().Cast<ItemSlot>())
            {
                elements[itemSlot] =
                    new EquipmentBuilderStub(itemSlot.ToString());
            }
            Equipment = new EquipmentBuilderCollectionStub(elements);
        }

        public IEquipmentBuilderCollection Equipment { get; }

        public IEquipmentBuilder LocalHand =>
            new EquipmentBuilderStub("Local Hand");
    }
}