using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilderAdapter : ICoreStatBuilder
    {
        private readonly IStatBuilder _statBuilder;
        private readonly IConditionBuilder _conditionBuilder;
        private readonly Func<IStat, IStat> _statConverter;

        public StatBuilderAdapter(IStatBuilder statBuilder, IConditionBuilder conditionBuilder = null)
            : this(statBuilder, conditionBuilder, Funcs.Identity)
        {
        }

        private StatBuilderAdapter(
            IStatBuilder statBuilder, IConditionBuilder conditionBuilder, Func<IStat, IStat> statConverter)
        {
            _statBuilder = statBuilder;
            _conditionBuilder = conditionBuilder;
            _statConverter = statConverter;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new StatBuilderAdapter(_statBuilder.Resolve(context), _conditionBuilder?.Resolve(context), _statConverter);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new StatBuilderAdapter(_statBuilder.For(entityBuilder), _conditionBuilder, _statConverter);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
        {
            var (statBuilder, conditionValueConverter) = BuildCondition(parameters);
            var statBuilderResults = statBuilder.Build(parameters);
            foreach (var (stats, modifierSource, statValueConverter) in statBuilderResults)
            {
                var convertedStats = stats.Select(_statConverter).ToList();
                IValueBuilder ConvertValue(IValueBuilder v) => conditionValueConverter(statValueConverter(v));
                yield return new StatBuilderResult(convertedStats, modifierSource, ConvertValue);
            }
        }

        private (IStatBuilder, ValueConverter) BuildCondition(BuildParameters parameters)
        {
            if (_conditionBuilder is null)
            {
                return (_statBuilder, Funcs.Identity);
            }

            var result = _conditionBuilder.Build(parameters);
            var statBuilder = result.StatConverter(_statBuilder);
            if (result.HasValue)
            {
                return (statBuilder, v => v.If(result.Value));
            }
            return (statBuilder, Funcs.Identity);
        }
    }
}