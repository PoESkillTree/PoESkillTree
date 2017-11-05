using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Matching;

namespace PoESkillTree.Computation.Console.Builders
{
    public class GroundEffectBuilderStub : EffectBuilderStub, IGroundEffectBuilder
    {
        public GroundEffectBuilderStub(string stringRepresentation, 
            Resolver<IEffectBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }
    }


    public class GroundEffectBuildersStub : IGroundEffectBuilders
    {
        public IGroundEffectBuilder Consecrated =>
            new GroundEffectBuilderStub("Consecrated Ground", (current, _) => current);
    }
}