using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class FlagStatBuilderStub : StatBuilderStub, IFlagStatBuilder
    {
        public FlagStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public IConditionBuilder IsSet => CreateCondition(This, o => $"{o} is set");

        public IStatBuilder Effect => CreateStat(This, o => $"Effect of {o}");
        public IStatBuilder Duration => CreateStat(This, o => $"Duration of {o}");
    }


    public class FlagStatBuildersStub : IFlagStatBuilders
    {
        private static IFlagStatBuilder Create(string s) 
            => new FlagStatBuilderStub(s, (c, _) => c);

        public IFlagStatBuilder Onslaught => Create("Onslaught");
        public IFlagStatBuilder UnholyMight => Create("Unholy Might");
        public IFlagStatBuilder Phasing => Create("Phasing");

        public IFlagStatBuilder IgnoreMovementSpeedPenalties =>
            Create("Ignore movement speed penalties from equipped armor");
    }
}