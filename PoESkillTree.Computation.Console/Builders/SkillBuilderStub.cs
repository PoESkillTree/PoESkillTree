using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class SkillBuilderStub : BuilderStub, ISkillBuilder
    {
        private readonly Resolver<ISkillBuilder> _resolver;

        public SkillBuilderStub(string stringRepresentation, Resolver<ISkillBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private ISkillBuilder This => this;

        public IActionBuilder Cast =>
            Create<IActionBuilder, ISkillBuilder>(
                ActionBuilderStub.BySelf,
                This, o => $"{o} cast");

        public IStatBuilder Instances =>
            CreateStat(This, o => $"{o} instance count");

        public IConditionBuilder HasInstance =>
            CreateCondition(This, o => $"{o} has any instances");

        public ValueBuilder SkillId =>
            new ValueBuilder(CreateValue(This, o => $"{o}.SkillId"));

        public ISkillBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class SkillBuilderCollectionStub : BuilderCollectionStub<ISkillBuilder>,
        ISkillBuilderCollection
    {
        public SkillBuilderCollectionStub(
            string stringRepresentation, Resolver<IBuilderCollection> resolver)
            : base(new SkillBuilderStub("Skill", (c, _) => c), stringRepresentation, resolver)
        {
        }

        private IBuilderCollection This => this;

        public IStatBuilder CombinedInstances =>
            CreateStat(This, o => $"{o} combined instance count");

        public IActionBuilder Cast =>
            Create<IActionBuilder, IBuilderCollection>(
                ActionBuilderStub.BySelf,
                This, o => $"{o} cast");
    }


    public class SkillBuildersStub : ISkillBuilders
    {
        private static ISkillBuilder Create(string s)
            => new SkillBuilderStub(s, (c, _) => c);

        public ISkillBuilderCollection this[params IKeywordBuilder[] keywords] =>
            (ISkillBuilderCollection) Create<IBuilderCollection, IKeywordBuilder>(
                (s, r) => new SkillBuilderCollectionStub(s, r),
                keywords,
                os => $"Skills.Where(has keywords [{string.Join(", ", os)}])");

        public ISkillBuilder MainSkill => Create("Main skill");

        public ISkillBuilder SummonSkeleton => Create("Summon Skeleton");

        public ISkillBuilder VaalSummonSkeletons => Create("Vaal Summon Skeletons");

        public ISkillBuilder RaiseSpectre => Create("Raise Spectre");

        public ISkillBuilder RaiseZombie => Create("Raise Zombie");

        public ISkillBuilder DetonateMines => Create("Detonate Mines");
    }
}