using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EquipmentBuilderStub : BuilderStub, IEquipmentBuilder
    {
        private readonly Resolver<IEquipmentBuilder> _resolver;

        public EquipmentBuilderStub(string stringRepresentation, Resolver<IEquipmentBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private IEquipmentBuilder This => this;

        public IConditionBuilder Has(Tags tag) =>
            CreateCondition(This, o => $"{o} has tag {tag}");

        public IConditionBuilder Has(FrameType frameType) =>
            CreateCondition(This, o => $"{o} has frame type {frameType}");

        public IConditionBuilder HasItem =>
            CreateCondition(This, o => $"{o} has item");

        public IConditionBuilder IsCorrupted =>
            CreateCondition(This, o => $"{o} is corrupted");

        public IFlagStatBuilder AppliesToSelf =>
            CreateFlagStat(This, o => $"{o} applies to self");

        public IFlagStatBuilder AppliesToMinions =>
            CreateFlagStat(This, o => $"{o} applies to minions");

        public IEquipmentBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class EquipmentBuilderCollectionStub : BuilderCollectionStub<IEquipmentBuilder>,
        IEquipmentBuilderCollection
    {
        private readonly IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> _elements;

        public EquipmentBuilderCollectionStub(IReadOnlyDictionary<ItemSlot, IEquipmentBuilder> elements)
            : base(new EquipmentBuilderStub("Item", (c, _) => c), "{Equipped Items}", (c, _) => c)
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
                    new EquipmentBuilderStub(itemSlot.ToString(), (c, _) => c);
            }

            Equipment = new EquipmentBuilderCollectionStub(elements);
        }

        public IEquipmentBuilderCollection Equipment { get; }
    }
}