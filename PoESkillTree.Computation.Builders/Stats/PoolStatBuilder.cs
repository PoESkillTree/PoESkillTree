using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
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
            FromIdentity($"{_pool}.Recharge.StartedRecently", typeof(bool), true).IsSet;
    }

    internal class RegenStatBuilder : StatBuilder, IRegenStatBuilder
    {
        private readonly Pool _pool;

        public RegenStatBuilder(IStatFactory statFactory, Pool pool)
            : base(statFactory, new LeafCoreStatBuilder(e => statFactory.Regen(pool, e)))
        {
            _pool = pool;
        }

        public IStatBuilder Percent => FromIdentity($"{_pool}.Regen.Percent", typeof(int));
        public IStatBuilder TargetPool => With(new LeafCoreStatBuilder(e => StatFactory.RegenTargetPool(_pool, e)));
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
            FromCore(new StatBuilderAdapter(damage.WithHits).WithStatConverter(StatFactory.LeechPercentage));

        public IStatBuilder RateLimit => FromIdentity($"{_pool}.Leech.RateLimit", typeof(int));
        public IStatBuilder Rate => FromIdentity($"{_pool}.Leech.Rate", typeof(double));
        public IStatBuilder TargetPool => FromIdentity($"{_pool}.Leech.TargetPool", typeof(Pool));
    }
}