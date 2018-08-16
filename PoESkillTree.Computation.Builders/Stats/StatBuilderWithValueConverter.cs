using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class StatBuilderWithValueConverter : ICoreStatBuilder
    {
        private readonly ICoreStatBuilder _inner;
        private readonly IEntityBuilder _entity;
        private readonly Func<BuildParameters, Entity, IValue> _buildValue;
        private readonly Func<IValueBuilder, IValueBuilder, IValueBuilder> _combineValues;

        public StatBuilderWithValueConverter(
            ICoreStatBuilder inner, Func<BuildParameters, Entity, IValue> buildValue,
            Func<IValueBuilder, IValueBuilder, IValueBuilder> combineValues)
            : this(inner, new ModifierSourceEntityBuilder(), buildValue, combineValues)
        {
        }

        private StatBuilderWithValueConverter(
            ICoreStatBuilder inner, IEntityBuilder entity, Func<BuildParameters, Entity, IValue> buildValue,
            Func<IValueBuilder, IValueBuilder, IValueBuilder> combineValues)
        {
            _inner = inner;
            _entity = entity;
            _buildValue = buildValue;
            _combineValues = combineValues;
        }

        public ICoreStatBuilder Resolve(ResolveContext context)
            => new StatBuilderWithValueConverter(_inner.Resolve(context), _entity.Resolve(context), _buildValue,
                _combineValues);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder)
            => new StatBuilderWithValueConverter(_inner, entityBuilder, _buildValue, _combineValues);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
        {
            var results = new List<StatBuilderResult>();
            var entities = _entity.Build(parameters.ModifierSourceEntity);
            foreach (var entity in entities)
            {
                var resultsForEntity = _inner.WithEntity(new EntityBuilder(entity))
                    .Build(parameters)
                    .Select(r
                        => new StatBuilderResult(r.Stats, r.ModifierSource, r.ValueConverter.AndThen(ConvertValue)));
                results.AddRange(resultsForEntity);

                IValueBuilder ConvertValue(IValueBuilder left)
                {
                    var right = new ValueBuilderImpl(_buildValue(parameters, entity));
                    return _combineValues(left, right);
                }
            }
            return results;
        }
    }
}