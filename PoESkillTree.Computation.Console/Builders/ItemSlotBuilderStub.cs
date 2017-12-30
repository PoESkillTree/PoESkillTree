using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ItemSlotBuilderStub : BuilderStub, IItemSlotBuilder
    {
        private readonly Resolver<IItemSlotBuilder> _resolver;

        public ItemSlotBuilderStub(string stringRepresentation, Resolver<IItemSlotBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IItemSlotBuilder Resolve(ResolveContext context) => _resolver(this, context);
    }

    public class ItemSlotBuildersStub : IItemSlotBuilders
    {
        public IItemSlotBuilder From(ItemSlot slot)
        {
            return new ItemSlotBuilderStub(slot.ToString(), (c, _) => c);
        }
    }
}