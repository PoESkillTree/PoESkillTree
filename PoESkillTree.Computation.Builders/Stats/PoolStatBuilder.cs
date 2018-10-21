using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Computation.Common;
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

        public IPoolStatBuilder From(Pool pool) => new PoolStatBuilder(StatFactory, CoreBuilder.Create(pool));
    }

    public class PoolStatBuilder : StatBuilderWithPool, IPoolStatBuilder
    {
        public PoolStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, "")
        {
        }

        private PoolStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, "")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new PoolStatBuilder(StatFactory, coreStatBuilder, pool);

        public new IPoolStatBuilder For(IEntityBuilder entity) =>
            (IPoolStatBuilder) base.For(entity);

        public IRegenStatBuilder Regen => new RegenStatBuilder(StatFactory, Pool);
        public IRechargeStatBuilder Recharge => new RechargeStatBuilder(StatFactory, Pool);
        public IStatBuilder RecoveryRate => FromIdentity(typeof(double));
        public IStatBuilder Cost => FromIdentity(typeof(int));
        public IStatBuilder Reservation => FromIdentity(typeof(int));
        public ILeechStatBuilder Leech => new LeechStatBuilder(StatFactory, Pool);
        public IStatBuilder InstantLeech => FromIdentity(typeof(bool));
        public IStatBuilder Gain => FromIdentity(typeof(int));

        public IConditionBuilder IsFull =>
            (Reservation.Value <= 0).And(FromIdentity(typeof(bool), UserSpecifiedValue()).IsSet);

        public IConditionBuilder IsLow =>
            (Reservation.Value >= 0.65 * Value).Or(FromIdentity(typeof(bool), UserSpecifiedValue()).IsSet);

        public Pool BuildPool() => Pool.Build();
    }

    internal class RechargeStatBuilder : StatBuilderWithPool, IRechargeStatBuilder
    {
        public RechargeStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Recharge")
        {
        }

        private RechargeStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Recharge")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new RechargeStatBuilder(StatFactory, coreStatBuilder, pool);

        public IStatBuilder Start => FromIdentity(typeof(double));

        public IConditionBuilder StartedRecently => FromIdentity(typeof(bool), UserSpecifiedValue()).IsSet;
    }

    internal class RegenStatBuilder : StatBuilderWithPool, IRegenStatBuilder
    {
        public RegenStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Regen")
        {
        }

        private RegenStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Regen")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new RegenStatBuilder(StatFactory, coreStatBuilder, pool);

        public IStatBuilder Percent => FromIdentity(typeof(int));

        public IStatBuilder TargetPool =>
            new StatBuilder(StatFactory, new CoreStatBuilderFromCoreBuilder<Pool>(Pool, StatFactory.RegenTargetPool));
    }

    internal class LeechStatBuilder : StatBuilderWithPool, ILeechStatBuilder
    {
        public LeechStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Leech")
        {
        }

        private LeechStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Leech")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new LeechStatBuilder(StatFactory, coreStatBuilder, pool);

        public new ILeechStatBuilder Resolve(ResolveContext context) => (ILeechStatBuilder) base.Resolve(context);

        public IStatBuilder Of(IDamageRelatedStatBuilder damage)
        {
            var damageCoreBuilder = new StatBuilderAdapter(damage.WithHits);
            var coreBuilder = new ParametrisedCoreStatBuilder<ICoreBuilder<Pool>>(damageCoreBuilder, Pool,
                (p, s) => StatFactory.CopyWithSuffix(s, $"LeechTo({p.Build()})", typeof(int)));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        public IStatBuilder RateLimit => FromIdentity(typeof(int));
        public IStatBuilder Rate => FromIdentity(typeof(double));

        public IStatBuilder TargetPool =>
            new StatBuilder(StatFactory, new CoreStatBuilderFromCoreBuilder<Pool>(Pool, StatFactory.LeechTargetPool));
    }

    public abstract class StatBuilderWithPool : StatBuilder
    {
        private readonly string _identitySuffix;
        protected ICoreBuilder<Pool> Pool { get; }

        protected StatBuilderWithPool(IStatFactory statFactory, ICoreBuilder<Pool> pool, string identitySuffix)
            : this(statFactory,
                new CoreStatBuilderFromCoreBuilder<Pool>(pool,
                    (e, p) => statFactory.FromIdentity(p.ToString() + identitySuffix, e, typeof(int))),
                pool,
                identitySuffix)
        {
        }

        protected StatBuilderWithPool(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool, string identitySuffix)
            : base(statFactory, coreStatBuilder)
        {
            Pool = pool;
            _identitySuffix = identitySuffix;
        }

        protected override IStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            Create(coreStatBuilder, Pool);

        public override IStatBuilder Resolve(ResolveContext context) =>
            Create(CoreStatBuilder.Resolve(context), Pool.Resolve(context));

        protected abstract IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool);

        protected IStatBuilder FromIdentity(
            Type dataType, ExplicitRegistrationType explicitRegistrationType = null,
            [CallerMemberName] string identitySuffix = null)
        {
            return new StatBuilder(StatFactory, new CoreStatBuilderFromCoreBuilder<Pool>(Pool,
                (e, p) => StatFactory.FromIdentity($"{p}{_identitySuffix}.{identitySuffix}", e, dataType,
                    explicitRegistrationType)));
        }
    }
}