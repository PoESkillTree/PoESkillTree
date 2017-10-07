using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ItemSlotBuilderStub : BuilderStub, IItemSlotBuilder
    {
        private readonly Resolver<IItemSlotBuilder> _resolver;

        public ItemSlotBuilderStub(string stringRepresentation, 
            Resolver<IItemSlotBuilder> resolver) 
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IItemSlotBuilder Resolve(IMatchContext<IValueBuilder> valueContext) =>
            _resolver(this, valueContext);
    }
}