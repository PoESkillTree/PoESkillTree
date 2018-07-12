using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class ParametrisedCoreStatBuilder<TParameter> : ICoreStatBuilder
        where TParameter : IResolvable<TParameter>
    {
        private readonly ICoreStatBuilder _inner;
        private readonly TParameter _parameter;
        private readonly Func<TParameter, IStat, IEnumerable<IStat>> _statConverter;

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, TParameter parameter,
            Func<TParameter, IStat, IStat> statConverter)
            : this(inner, parameter, (ps, s) => new[] { statConverter(ps, s) })
        {
        }

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, TParameter parameter,
            Func<TParameter, IStat, IEnumerable<IStat>> statConverter)
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

        public IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            from result in _inner.Build(parameters, originalModifierSource)
            let stats = result.Stats.SelectMany(s => _statConverter(_parameter, s))
            select new StatBuilderResult(stats.ToList(), result.ModifierSource, result.ValueConverter);
    }

    internal class ParametrisedCoreStatBuilder<T1, T2> : ICoreStatBuilder
        where T1 : IResolvable<T1>
        where T2 : IResolvable<T2>
    {
        private readonly ICoreStatBuilder _inner;
        private readonly T1 _parameter1;
        private readonly T2 _parameter2;
        private readonly Func<T1, T2, IStat, IEnumerable<IStat>> _statConverter;

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, T1 parameter1, T2 parameter2,
            Func<T1, T2, IStat, IEnumerable<IStat>> statConverter)
        {
            _inner = inner;
            _parameter1 = parameter1;
            _parameter2 = parameter2;
            _statConverter = statConverter;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new ParametrisedCoreStatBuilder<T1, T2>(_inner.Resolve(context),
                _parameter1.Resolve(context), _parameter2.Resolve(context), _statConverter);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new ParametrisedCoreStatBuilder<T1, T2>(_inner.WithEntity(entityBuilder), _parameter1, _parameter2,
                _statConverter);

        public IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            from result in _inner.Build(parameters, originalModifierSource)
            let stats = result.Stats.SelectMany(s => _statConverter(_parameter1, _parameter2, s))
            select new StatBuilderResult(stats.ToList(), result.ModifierSource, result.ValueConverter);
    }
}