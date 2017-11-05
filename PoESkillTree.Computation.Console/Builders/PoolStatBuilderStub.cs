using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class PoolStatBuilderStub : StatBuilderStub, IPoolStatBuilder
    {
        public PoolStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }

        public IRegenStatBuilder Regen =>
            (IRegenStatBuilder) Create(
                (s, r) => new RegenStatBuilderStub(s, r),
                This, o => $"{o} regeneration");

        public IRechargeStatBuilder Recharge =>
            (IRechargeStatBuilder) Create(
                (s, r) => new RechargeStatBuilderStub(s, r),
                This, o => $"{o} recharge");

        public IStatBuilder RecoveryRate => CreateStat(This, o => $"{o} recovery rate");
        public IStatBuilder Reservation => CreateStat(This, o => $"{o} reservation");

        public ILeechStatBuilder Leech =>
            Create<ILeechStatBuilder, IStatBuilder>(
                (s, r) => new LeechStatBuilderStub(s, r),
                This, o => $"{o} Leech");

        public IFlagStatBuilder InstantLeech =>
            CreateFlagStat(This, o => $"{o} gained from Leech instantly");

        public IStatBuilder Gain => CreateStat(This, o => $"{o} gain");

        public IConditionBuilder IsFull => CreateCondition(This, o => $"{o} is full");
        public IConditionBuilder IsLow => CreateCondition(This, o => $"{o} is low");
    }


    public class PoolStatBuildersStub : IPoolStatBuilders
    {
        private static IPoolStatBuilder Create(string stringRepresentation) => 
            new PoolStatBuilderStub(stringRepresentation, (c, _) => c);

        public IPoolStatBuilder Life => Create("Life");
        public IPoolStatBuilder Mana => Create("Mana");
        public IPoolStatBuilder EnergyShield => Create("Energy Shield");
    }


    public class RechargeStatBuilderStub : StatBuilderStub, IRechargeStatBuilder
    {
        public RechargeStatBuilderStub(string stringRepresentation, 
            Resolver<IStatBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Start => CreateStat(This, o => $"Start of {o}");

        public IConditionBuilder StartedRecently => 
            CreateCondition(This, o => $"{o} started recently");
    }


    public class RegenStatBuilderStub : StatBuilderStub, IRegenStatBuilder
    {
        public RegenStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Percent => CreateStat(This, o => $"Percent {o}");

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            CreateFlagStat(This, (IStatBuilder) stat, (o1, o2) => $"{o1} applies to {o2} instead");
    }


    public class LeechStatBuilderStub : BuilderStub, ILeechStatBuilder
    {
        private readonly Resolver<ILeechStatBuilder> _resolver;

        public LeechStatBuilderStub(string stringRepresentation, 
            Resolver<ILeechStatBuilder> resolver) 
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        private ILeechStatBuilder This => this;

        public IStatBuilder Of(IDamageStatBuilder damage) =>
            CreateStat(This, (IStatBuilder) damage, (o1, o2) => $"{o2} {o1}");

        public IStatBuilder RateLimit =>
            CreateStat(This, o => $"Maximum {o} rate per second");
        public IStatBuilder Rate =>
            CreateStat(This, o => $"{o} per second");

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            CreateFlagStat(This, (IStatBuilder) stat, (o1, o2) => $"{o1} applies to {o2} instead");

        public ILeechStatBuilder To(IEntityBuilder entity) =>
            Create((s, r) => new LeechStatBuilderStub(s, r),
                This, entity, (o1, o2) => $"{o1} leeched to {o2}");

        public IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType) =>
            CreateFlagStat(This, (IKeywordBuilder) damageType, 
                (o1, o2) => $"{o1} recovers based on {o2} instead");

        public ILeechStatBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}