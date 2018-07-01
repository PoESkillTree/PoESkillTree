using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Console.Builders
{
    public class GroundEffectBuildersStub : IGroundEffectBuilders
    {
        public IEffectBuilder Consecrated =>
            new EffectBuilderStub("Consecrated Ground", (current, _) => current);
    }
}