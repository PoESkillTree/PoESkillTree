using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Builders.Equipment
{
    public class EquipmentBuilder : IEquipmentBuilder
    {
        private readonly IStatFactory _statFactory;
        private readonly ItemSlot _slot;

        public EquipmentBuilder(IStatFactory statFactory, ItemSlot slot)
        {
            _statFactory = statFactory;
            _slot = slot;
        }

        public IEquipmentBuilder Resolve(ResolveContext context) => this;

        public IStatBuilder ItemTags
            => StatBuilderUtils.FromIdentity(_statFactory, $"{_slot}.ItemTags", typeof(Tags));

        public IConditionBuilder Has(Tags tag)
            => ValueConditionBuilder.Create(ItemTags.Value, v => ToTags(v).HasFlag(tag),
                v => $"((Tags) {v}).HasFlag({tag})");

        private static Tags ToTags(NodeValue? value)
            => value is NodeValue v ? TagsExtensions.DecodeFromDouble(v.Single) : Tags.Default;

        public IStatBuilder ItemClass
            => StatBuilderUtils.FromIdentity(_statFactory, $"{_slot}.ItemClass", typeof(ItemClass));

        public IConditionBuilder Has(ItemClass itemClass) => ItemClass.Value.Eq((int) itemClass);

        public IStatBuilder FrameType
            => StatBuilderUtils.FromIdentity(_statFactory, $"{_slot}.ItemFrameType", typeof(FrameType));

        public IConditionBuilder Has(FrameType frameType) => FrameType.Value.Eq((int) frameType);

        public IConditionBuilder HasItem
            => ValueConditionBuilder.Create(ItemTags.Value, v => v.HasValue, v => $"{v}.HasValue");

        public IStatBuilder Corrupted
            => StatBuilderUtils.FromIdentity(_statFactory, $"{_slot}.ItemIsCorrupted", typeof(bool));
    }
}