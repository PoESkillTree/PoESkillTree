using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class FlagStatBuilderStub : StatBuilderStub, IFlagStatBuilder
    {
        public FlagStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IConditionBuilder IsSet =>
            new ConditionBuilderStub($"{this} is set");

        public IStatBuilder Effect => Create($"Effect of {this}");
        public IStatBuilder Duration => Create($"Duration of {this}");
    }


    public class FlagStatBuildersStub : IFlagStatBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public FlagStatBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        private IFlagStatBuilder Create(string s)
        {
            return new FlagStatBuilderStub(s, _conditionBuilders);
        }

        public IFlagStatBuilder Onslaught => Create("Onslaught");
        public IFlagStatBuilder UnholyMight => Create("Unholy Might");
        public IFlagStatBuilder Phasing => Create("Phasing");

        public IFlagStatBuilder IgnoreMovementSpeedPenalties =>
            Create("Ignore movement speed penalties from equipped armor");
    }
}