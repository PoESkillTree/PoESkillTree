using PoESkillTree.Computation.Parsing.Builders.Equipment;
using PoESkillTree.Computation.Parsing.Builders.Matching;

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

        public IItemSlotBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}