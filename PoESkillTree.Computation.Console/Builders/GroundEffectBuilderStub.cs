using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Effects;

namespace PoESkillTree.Computation.Console.Builders
{
    public class GroundEffectBuilderStub : EffectBuilderStub, IGroundEffectBuilder
    {
        public GroundEffectBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }
    }


    public class GroundEffectBuildersStub : IGroundEffectBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public GroundEffectBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IGroundEffectBuilder Consecrated =>
            new GroundEffectBuilderStub("Consecrated", _conditionBuilders);
    }
}