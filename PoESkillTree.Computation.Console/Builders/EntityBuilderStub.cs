using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EntityBuilderStub : BuilderStub, IEntityBuilder
    {
        private readonly Resolver<IEntityBuilder> _resolver;

        public EntityBuilderStub(string stringRepresentation, Resolver<IEntityBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public static EntityBuilderStub Self() => new EntityBuilderStub("Self", (c, _) => c);
        public static EntityBuilderStub Any() => new EntityBuilderStub("Any Entity", (c, _) => c);

        protected IEntityBuilder This => this;

        public IDamageStatBuilder Stat(IDamageStatBuilder stat) =>
            CreateDamageStat(This, (IStatBuilder) stat, (o1, o2) => $"{o2} for {o1}");

        public IFlagStatBuilder Stat(IFlagStatBuilder stat) =>
            CreateFlagStat(This, (IStatBuilder) stat, (o1, o2) => $"{o2} for {o1}");

        public IPoolStatBuilder Stat(IPoolStatBuilder stat) =>
            CreatePoolStat(This, (IStatBuilder) stat, (o1, o2) => $"{o2} for {o1}");

        public IStatBuilder Stat(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"{o2} for {o1}");

        public IStatBuilder Level =>
            CreateStat(This, o => $"{o} Level");

        public IEntityBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class EntityBuildersStub : IEntityBuilders
    {
        public IEntityBuilder Self => EntityBuilderStub.Self();
        public IEnemyBuilder Enemy => new EnemyBuilderStub();
        public IEntityBuilder Ally => new EntityBuilderStub("Ally", (c, _) => c);
        public IEntityBuilder ModifierSource => new EntityBuilderStub("Modifier Source", (c, _) => c);

        public ISkillEntityBuilder Totem => new SkillEntityBuilderStub("Totem", (c, _) => c);

        public ISkillEntityBuilder Minion => new SkillEntityBuilderStub("Minion", (c, _) => c);

        public IEntityBuilder Any => EntityBuilderStub.Any();
    }


    public class EnemyBuilderStub : EntityBuilderStub, IEnemyBuilder
    {
        public EnemyBuilderStub()
            : base("Enemy", (c, _) => c)
        {
        }

        public IConditionBuilder IsNearby =>
            CreateCondition(This, o => $"{o} is nearby");

        public IConditionBuilder IsRare =>
            CreateCondition(This, o => $"{o} is rare");

        public IConditionBuilder IsUnique =>
            CreateCondition(This, o => $"{o} is unique");

        public IConditionBuilder IsRareOrUnique =>
            CreateCondition(This, o => $"{o} is rare or unique");
    }


    public class SkillEntityBuilderStub : EntityBuilderStub, ISkillEntityBuilder
    {
        public SkillEntityBuilderStub(string stringRepresentation,
            Resolver<IEntityBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        private static IEntityBuilder Construct(string stringRepresentation,
            Resolver<IEntityBuilder> resolver)
            => new SkillEntityBuilderStub(stringRepresentation, resolver);

        public ISkillEntityBuilder With(IKeywordBuilder keyword) =>
            (ISkillEntityBuilder) Create(
                Construct, This, keyword,
                (o1, o2) => $"{o1} with {o2}");

        public ISkillEntityBuilder With(params IKeywordBuilder[] keywords) =>
            (ISkillEntityBuilder) Create(
                Construct, This, keywords,
                (o1, os) => $"{o1} with ({string.Join(", ", os)})");

        public ISkillEntityBuilder From(ISkillBuilder skill) =>
            (ISkillEntityBuilder) Create(
                Construct, This, skill,
                (o1, o2) => $"{o1} from {o2}");
    }
}