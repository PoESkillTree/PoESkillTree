using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Conditions;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Computation.Builders.Actions
{
    public class ActionBuilder : IActionBuilder
    {
        private const int RecentlySeconds = 4;

        protected IStatFactory StatFactory { get; }
        private readonly ICoreBuilder<string> _identity;
        private readonly IEntityBuilder _entity;

        public ActionBuilder(IStatFactory statFactory, ICoreBuilder<string> identity, IEntityBuilder entity)
        {
            StatFactory = statFactory;
            _identity = identity;
            _entity = entity;
        }

        public IActionBuilder Resolve(ResolveContext context) =>
            new ActionBuilder(StatFactory, _identity.Resolve(context), _entity.Resolve(context));

        public IActionBuilder By(IEntityBuilder source) =>
            new ActionBuilder(StatFactory, _identity, source);

        public IConditionBuilder On =>
            new StatConvertingConditionBuilder(b => new StatBuilder(StatFactory,
                new ParametrisedCoreStatBuilder<ICoreBuilder<string>, IEntityBuilder>(
                    new StatBuilderAdapter(b), _identity, _entity, ConvertStat)),
                c => Resolve(c).On);

        private IEnumerable<IStat> ConvertStat(ICoreBuilder<string> identity, IEntityBuilder entity, IStat stat)
        {
            var builtIdentity = identity.Build();
            return from e in entity.Build(stat.Entity)
                   let i = $"On({builtIdentity}).By({e})"
                   let registrationType = GainOnAction(stat, builtIdentity, e)
                   select StatFactory.CopyWithSuffix(stat, i, stat.DataType, registrationType);
        }

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            new ValueConditionBuilder(ps => BuildInPastXSecondsValue(ps, seconds),
                c => Resolve(c).InPastXSeconds(seconds.Resolve(c)));

        private IValue BuildInPastXSecondsValue(BuildParameters parameters, IValueBuilder seconds)
        {
            var builtEntity = BuildEntity(parameters, _entity);
            var recentOccurencesStat = BuildRecentOccurencesStat(builtEntity);
            var lastOccurenceStat = BuildLastOccurenceStat(builtEntity);
            var secondsValue = seconds.Build(parameters);
            return new ConditionalValue(Calculate,
                $"({RecentlySeconds} <= {secondsValue} && {recentOccurencesStat} > 0) " +
                $"|| {lastOccurenceStat} <= {secondsValue}");

            bool Calculate(IValueCalculationContext context)
            {
                NodeValue? threshold = secondsValue.Calculate(context);
                if (RecentlySeconds <= threshold && context.GetValue(recentOccurencesStat) > 0)
                    return true;
                return context.GetValue(lastOccurenceStat) <= threshold;
            }
        }

        public IConditionBuilder Recently =>
            InPastXSeconds(new ValueBuilderImpl(RecentlySeconds));

        public ValueBuilder CountRecently =>
            new ValueBuilder(new ValueBuilderImpl(BuildCountRecentlyValue, c => Resolve(c).CountRecently));

        private IValue BuildCountRecentlyValue(BuildParameters parameters) =>
            new StatValue(BuildRecentOccurencesStat(BuildEntity(parameters, _entity)));

        private IStat BuildLastOccurenceStat(Entity entity) =>
            StatFactory.FromIdentity($"{Build()}.LastOccurence", entity, typeof(int), UserSpecifiedValue());

        private IStat BuildRecentOccurencesStat(Entity entity) =>
            StatFactory.FromIdentity($"{Build()}.RecentOccurences", entity, typeof(int), UserSpecifiedValue());

        private static Entity BuildEntity(BuildParameters parameters, IEntityBuilder entity) =>
            entity.Build(parameters.ModifierSourceEntity).Single();

        public string Build() => _identity.Build();
    }
}