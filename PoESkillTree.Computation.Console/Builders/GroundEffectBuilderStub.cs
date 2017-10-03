using PoESkillTree.Computation.Parsing.Builders.Effects;

namespace PoESkillTree.Computation.Console.Builders
{
    public class GroundEffectBuilderStub : EffectBuilderStub, IGroundEffectBuilder
    {
        public GroundEffectBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }
    }


    public class GroundEffectBuildersStub : IGroundEffectBuilders
    {
        public IGroundEffectBuilder Consecrated =>
            new GroundEffectBuilderStub("Consecrated");
    }
}