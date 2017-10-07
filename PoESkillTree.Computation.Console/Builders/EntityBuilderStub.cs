using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;
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

        protected IEntityBuilder This => this;

        public IConditionBuilder
            HitByInPastXSeconds(IDamageTypeBuilder damageType, IValueBuilder seconds) =>
            CreateCondition(This, (IKeywordBuilder) damageType, seconds,
                (o1, o2, o3) => $"{o1} hit by {o2} Damage in the past {o3} seconds");

        public IConditionBuilder
            HitByInPastXSeconds(IDamageTypeBuilder damageType, double seconds) =>
            CreateCondition(This, (IKeywordBuilder) damageType,
                (o1, o2) => $"{o1} hit by {o2} Damage in the past {seconds} seconds");

        public IConditionBuilder HitByRecently(IDamageTypeBuilder damageType) =>
            CreateCondition(This, (IKeywordBuilder) damageType,
                (o1, o2) => $"{o1} hit by {o2} Damage recently");

        public IDamageStatBuilder Stat(IDamageStatBuilder stat) =>
            (IDamageStatBuilder) Create<IStatBuilder, IEntityBuilder, IStatBuilder>(
                (s, r) => new DamageStatBuilderStub(s, r),
                This, stat, (o1, o2) => $"{o2} for {o1}");

        public IFlagStatBuilder Stat(IFlagStatBuilder stat) =>
            CreateFlagStat(This, (IStatBuilder) stat, (o1, o2) => $"{o2} for {o1}");

        public IPoolStatBuilder Stat(IPoolStatBuilder stat) =>
            (IPoolStatBuilder) Create<IStatBuilder, IEntityBuilder, IStatBuilder>(
                (s, r) => new PoolStatBuilderStub(s, r),
                This, stat, (o1, o2) => $"{o2} for {o1}");

        public IStatBuilder Stat(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"{o2} for {o1}");

        public IStatBuilder Level =>
            CreateStat(This, o => $"{o} Level");

        public IEntityBuilder Resolve(IMatchContext<IValueBuilder> valueContext) =>
            _resolver(this, valueContext);
    }


    public class EntityBuildersStub : IEntityBuilders
    {
        public ISelfBuilder Self => new SelfBuilderStub();
        public IEnemyBuilder Enemy => new EnemyBuilderStub();
        public IEntityBuilder Ally => new EntityBuilderStub("Ally", (c, _) => c);
        public IEntityBuilder Character => new EntityBuilderStub("Character", (c, _) => c);

        public ISkillEntityBuilder Totem => new SkillEntityBuilderStub("Totem", (c, _) => c);

        public ISkillEntityBuilder Minion => new SkillEntityBuilderStub("Minion", (c, _) => c);

        public IEntityBuilder Any => new EntityBuilderStub("Any Entity", (c, _) => c);
    }


    public class SelfBuilderStub : EntityBuilderStub, ISelfBuilder
    {
        public SelfBuilderStub() 
            : base("Self", (c, _) => c)
        {
        }
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