using PoESkillTree.Computation.Parsing.Builders.Damage;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageSourceBuilderStub : BuilderStub, IDamageSourceBuilder
    {
        public DamageSourceBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }
    }


    public class DamageSourceBuildersStub : IDamageSourceBuilders
    {
        public IDamageSourceBuilder Attack => new DamageSourceBuilderStub("Attack");

        public IDamageSourceBuilder Spell => new DamageSourceBuilderStub("Spell");

        public IDamageSourceBuilder Secondary => new DamageSourceBuilderStub("Secondary");

        public IDamageSourceBuilder DamageOverTime =>
            new DamageSourceBuilderStub("Damage over Time");
    }
}