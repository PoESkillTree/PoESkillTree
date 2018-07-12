using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class PoolStatBuilders : StatBuildersBase, IPoolStatBuilders
    {
        public PoolStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IPoolStatBuilder From(Pool pool) => new PoolStatBuilder(StatFactory, pool);
    }

    internal class PoolStatBuilder : StatBuilder, IPoolStatBuilder
    {
        private readonly Pool _pool;

        public PoolStatBuilder(IStatFactory statFactory, Pool pool)
            : this(statFactory, LeafCoreStatBuilder.FromIdentity(statFactory, pool.ToString(), typeof(int)), pool)
        {
        }

        private PoolStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, Pool pool)
            : base(statFactory, coreStatBuilder)
        {
            _pool = pool;
        }

        protected override IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            new PoolStatBuilder(StatFactory, coreStatBuilder, _pool);

        public new IPoolStatBuilder For(IEntityBuilder entity) =>
            (IPoolStatBuilder) base.For(entity);

        public IRegenStatBuilder Regen => new RegenStatBuilder(StatFactory, _pool);
        public IRechargeStatBuilder Recharge => new RechargeStatBuilder(StatFactory, _pool);
        public IStatBuilder RecoveryRate => FromIdentity($"{_pool}.RecoveryRate", typeof(double));
        public IStatBuilder Cost => FromIdentity($"{_pool}.Cost", typeof(int));
        public IStatBuilder Reservation => FromIdentity($"{_pool}.Reservation", typeof(int));
        public ILeechStatBuilder Leech => new LeechStatBuilder(StatFactory, _pool);
        public IFlagStatBuilder InstantLeech => FromIdentity($"{_pool}.InstantLeech", typeof(bool));
        public IStatBuilder Gain => FromIdentity($"{_pool}.Gain", typeof(int));

        public IConditionBuilder IsFull =>
            (Reservation.Value <= 0).And(FromIdentity($"{_pool}.IsFull", typeof(bool), UserSpecifiedValue()).IsSet);

        public IConditionBuilder IsLow =>
            (Reservation.Value >= 65).Or(FromIdentity($"{_pool}.IsLow", typeof(bool), UserSpecifiedValue()).IsSet);

        public Pool BuildPool() => _pool;

        public override string ToString() => _pool.ToString();
    }

    internal class RechargeStatBuilder : StatBuilder, IRechargeStatBuilder
    {
        private readonly Pool _pool;

        public RechargeStatBuilder(IStatFactory statFactory, Pool pool)
            : base(statFactory, LeafCoreStatBuilder.FromIdentity(statFactory, $"{pool}.Recharge", typeof(int)))
        {
            _pool = pool;
        }

        public IStatBuilder Start => FromIdentity($"{_pool}.Recharge.Start", typeof(double));

        public IConditionBuilder StartedRecently =>
            FromIdentity($"{_pool}.Recharge.StartedRecently", typeof(bool), UserSpecifiedValue()).IsSet;
    }

    internal class RegenStatBuilder : StatBuilder, IRegenStatBuilder
    {
        private readonly Pool _pool;

        public RegenStatBuilder(IStatFactory statFactory, Pool pool)
            : base(statFactory, new LeafCoreStatBuilder(e => statFactory.Regen(e, pool)))
        {
            _pool = pool;
        }

        public IStatBuilder Percent => FromIdentity($"{_pool}.Regen.Percent", typeof(int));
        public IStatBuilder TargetPool => With(new LeafCoreStatBuilder(e => StatFactory.RegenTargetPool(e, _pool)));
    }

    internal class LeechStatBuilder : StatBuildersBase, ILeechStatBuilder
    {
        private readonly Pool _pool;

        public LeechStatBuilder(IStatFactory statFactory, Pool pool) : base(statFactory)
        {
            _pool = pool;
        }

        public ILeechStatBuilder Resolve(ResolveContext context) => this;

        public IStatBuilder Of(IDamageStatBuilder damage) =>
            FromCore(new StatBuilderWithStatConverter(new StatBuilderAdapter(damage.WithHits),
                StatFactory.LeechPercentage));

        public IStatBuilder RateLimit => FromIdentity($"{_pool}.Leech.RateLimit", typeof(int));
        public IStatBuilder Rate => FromIdentity($"{_pool}.Leech.Rate", typeof(double));
        public IStatBuilder TargetPool => FromIdentity($"{_pool}.Leech.TargetPool", typeof(Pool));
    }
}