using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

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

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new StatBuilderAdapter(_statBuilder, _conditionBuilder, statConverter);

        public IValue BuildValue(Entity modifierSourceEntity) =>
            _statBuilder.Value.Build(modifierSourceEntity);

        public IReadOnlyList<StatBuilderResult> Build(
            ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var (statBuilder, conditionValueConverter) = BuildCondition(modifierSourceEntity);
            var (stats, modifierSource, statValueConverter) =
                statBuilder.Build(originalModifierSource, modifierSourceEntity).Single(); // TODO
            stats = stats.Select(_statConverter).ToList();
            IValueBuilder ConvertValue(IValueBuilder v) => conditionValueConverter(statValueConverter(v));
            return new[] { new StatBuilderResult(stats, modifierSource, ConvertValue) }; // TODO
        }

        private (IStatBuilder, ValueConverter) BuildCondition(Entity modifierSourceEntity)
        {
            if (_conditionBuilder is null)
            {
                return (_statBuilder, Funcs.Identity);
            }
            var (statConverter, value) = _conditionBuilder.Build(modifierSourceEntity);
            return (statConverter(_statBuilder), v => new ValueBuilderImpl(value).Multiply(v));
        }
    }
}