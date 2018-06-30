using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class PoolStatBuilderStub : StatBuilderStub, IPoolStatBuilder
    {
        public PoolStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
            : base(stringRepresentation, resolver)
        {
        }

        public new IPoolStatBuilder For(IEntityBuilder entity) =>
            (IPoolStatBuilder) Create((s, r) => new PoolStatBuilderStub(s, r),
                This, entity, (o1, o2) => $"{o1} for {o2}");

        public IRegenStatBuilder Regen =>
            (IRegenStatBuilder) Create(
                (s, r) => new RegenStatBuilderStub(s, r),
                This, o => $"{o} regeneration");

        public IRechargeStatBuilder Recharge =>
            (IRechargeStatBuilder) Create(
                (s, r) => new RechargeStatBuilderStub(s, r),
                This, o => $"{o} recharge");

        public IStatBuilder RecoveryRate => CreateStat(This, o => $"{o} recovery rate");
        public IStatBuilder Cost => CreateStat(This, o => $"{o} cost");
        public IStatBuilder Reservation => CreateStat(This, o => $"{o} reservation");

        public ILeechStatBuilder Leech =>
            Create<ILeechStatBuilder, IStatBuilder>(
                (s, r) => new LeechStatBuilderStub(s, r),
                This, o => $"{o}.Leech");

        public IFlagStatBuilder InstantLeech =>
            CreateFlagStat(This, o => $"{o} gained from Leech instantly");

        public IStatBuilder Gain => CreateStat(This, o => $"{o} gain");

        public IConditionBuilder IsFull => CreateCondition(This, o => $"{o} is full");
        public IConditionBuilder IsLow => CreateCondition(This, o => $"{o} is low");

        public Pool BuildPool() => default;

        public override IStatBuilder WithCondition(IConditionBuilder condition) =>
            CreatePoolStat(This, condition, (s, c) => $"{s} ({c})");
    }


    public class RechargeStatBuilderStub : StatBuilderStub, IRechargeStatBuilder
    {
        public RechargeStatBuilderStub(string stringRepresentation, Resolver<IStatBuilder> resolver)
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

        public IStatBuilder TargetPool => CreateStat(This, o1 => $"{o1}.TargetPool");
    }


    public class LeechStatBuilderStub : BuilderStub, ILeechStatBuilder
    {
        private readonly Resolver<ILeechStatBuilder> _resolver;

        public LeechStatBuilderStub(string stringRepresentation, Resolver<ILeechStatBuilder> resolver)
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
            CreateStat(This, o => $"{o}.Rate");

        public IStatBuilder TargetPool =>
            CreateStat(This, o => $"{o}.TargetPool");

        public ILeechStatBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }
}