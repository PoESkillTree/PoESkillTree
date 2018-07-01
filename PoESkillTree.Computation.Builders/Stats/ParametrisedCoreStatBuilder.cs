using System;
using System.Collections.Generic;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class ParametrisedCoreStatBuilder<TParameter> : ICoreStatBuilder
        where TParameter: IResolvable<TParameter>
    {
        private readonly ICoreStatBuilder _inner;
        private readonly TParameter _parameter;
        private readonly Func<TParameter, IStat, IStat> _statConverter;

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, TParameter parameter,
            Func<TParameter, IStat, IStat> statConverter)
        {
            _inner = inner;
            _parameter = parameter;
            _statConverter = statConverter;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new ParametrisedCoreStatBuilder<TParameter>(_inner.Resolve(context), _parameter.Resolve(context),
                _statConverter);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new ParametrisedCoreStatBuilder<TParameter>(_inner.WithEntity(entityBuilder), _parameter, _statConverter);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new ParametrisedCoreStatBuilder<TParameter>(_inner, _parameter, _statConverter.AndThen(statConverter));

        public IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            ApplyParameterToInner().Build(parameters, originalModifierSource);

        private ICoreStatBuilder ApplyParameterToInner() =>
            _inner.WithStatConverter(s => _statConverter(_parameter, s));
    }
}