using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class StatBuilderWithValueConverter : ICoreStatBuilder
    {
        private readonly ICoreStatBuilder _inner;
        private readonly IEntityBuilder _entity;
        private readonly Func<Entity, IValueBuilder> _createValue;
        private readonly Func<IValueBuilder, IValueBuilder, IValueBuilder> _combineValues;

        public StatBuilderWithValueConverter(
            ICoreStatBuilder inner, Func<Entity, IValueBuilder> createValue,
            Func<IValueBuilder, IValueBuilder, IValueBuilder> combineValues)
            : this(inner, new ModifierSourceEntityBuilder(), createValue, combineValues)
        {
        }

        private StatBuilderWithValueConverter(
            ICoreStatBuilder inner, IEntityBuilder entity, Func<Entity, IValueBuilder> createValue,
            Func<IValueBuilder, IValueBuilder, IValueBuilder> combineValues)
        {
            _inner = inner;
            _entity = entity;
            _createValue = createValue;
            _combineValues = combineValues;
        }

        public ICoreStatBuilder Resolve(ResolveContext context)
            => new StatBuilderWithValueConverter(_inner.Resolve(context), _entity,
                _createValue.AndThen(b => b.Resolve(context)), _combineValues);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder)
            => new StatBuilderWithValueConverter(_inner, entityBuilder, _createValue, _combineValues);

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

                IValueBuilder ConvertValue(IValueBuilder left) => _combineValues(left, _createValue(entity));
            }
            return results;
        }
    }
}