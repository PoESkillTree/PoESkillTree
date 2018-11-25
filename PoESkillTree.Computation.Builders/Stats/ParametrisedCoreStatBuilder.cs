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
        private readonly Func<BuildParameters, TParameter, IStat, IEnumerable<IStat>> _statConverter;

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, TParameter parameter,
            Func<BuildParameters, TParameter, IStat, IStat> statConverter)
            : this(inner, parameter, (ps, p, s) => new[] { statConverter(ps, p, s) })
        {
        }

        public ParametrisedCoreStatBuilder(ICoreStatBuilder inner, TParameter parameter,
            Func<BuildParameters, TParameter, IStat, IEnumerable<IStat>> statConverter)
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

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            from result in _inner.Build(parameters)
            let stats = result.Stats.SelectMany(s => _statConverter(parameters, _parameter, s))
            select new StatBuilderResult(stats.ToList(), result.ModifierSource, result.ValueConverter);
    }
}