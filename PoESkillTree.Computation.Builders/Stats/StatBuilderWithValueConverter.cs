using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Func<Entity, IValue> _buildValue;
        private readonly Func<IValueBuilder, IValueBuilder, IValueBuilder> _combineValues;

        public StatBuilderWithValueConverter(
            ICoreStatBuilder inner, Func<Entity, IValue> buildValue,
            Func<IValueBuilder, IValueBuilder, IValueBuilder> combineValues)
        {
            _inner = inner;
            _buildValue = buildValue;
            _combineValues = combineValues;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new StatBuilderWithValueConverter(_inner.Resolve(context), _buildValue, _combineValues);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new StatBuilderWithValueConverter(_inner.WithEntity(entityBuilder), _buildValue, _combineValues);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters,
            ModifierSource originalModifierSource)
        {
            return _inner.Build(parameters, originalModifierSource)
                .Select(r =>
                    new StatBuilderResult(r.Stats, r.ModifierSource, r.ValueConverter.AndThen(ConvertValue)));

            IValueBuilder ConvertValue(IValueBuilder left)
            {
                var right = new ValueBuilderImpl(_buildValue(parameters.ModifierSourceEntity));
                return _combineValues(left, right);
            }
        }
    }
}