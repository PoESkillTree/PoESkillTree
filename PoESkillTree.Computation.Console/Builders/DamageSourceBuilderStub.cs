using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageSourceBuilderStub : BuilderStub, IDamageSourceBuilder
    {
        private readonly Resolver<IDamageSourceBuilder> _resolver;

        public DamageSourceBuilderStub(string stringRepresentation,
            Resolver<IDamageSourceBuilder> resolver) 
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IDamageSourceBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class DamageSourceBuildersStub : IDamageSourceBuilders
    {
        private static IDamageSourceBuilder Create(string stringRepresentation) =>
            new DamageSourceBuilderStub(stringRepresentation, (current, _) => current);

        public IDamageSourceBuilder Attack => Create("Attack");

        public IDamageSourceBuilder Spell => Create("Spell");

        public IDamageSourceBuilder Secondary => Create("Secondary");

        public IDamageSourceBuilder DamageOverTime => Create("Damage over Time");
    }
}