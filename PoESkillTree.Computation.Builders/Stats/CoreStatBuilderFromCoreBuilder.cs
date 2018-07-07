using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class CoreStatBuilderFromCoreBuilder<T> : ICoreStatBuilder
    {
        private readonly ICoreBuilder<T> _coreBuilder;
        private readonly Func<Entity, T, IEnumerable<IStat>> _statFactory;
        private readonly IEntityBuilder _entityBuilder;

        public CoreStatBuilderFromCoreBuilder(
            ICoreBuilder<T> coreBuilder, Func<Entity, T, IStat> statFactory, IEntityBuilder entityBuilder = null)
            : this(coreBuilder, (e, t) => new []{statFactory(e, t)}, entityBuilder)
        {
        }

        public CoreStatBuilderFromCoreBuilder(
            ICoreBuilder<T> coreBuilder,
            Func<Entity, T, IEnumerable<IStat>> statFactory,
            IEntityBuilder entityBuilder = null)
        {
            _coreBuilder = coreBuilder;
            _statFactory = statFactory;
            _entityBuilder = entityBuilder ?? new ModifierSourceEntityBuilder();
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new CoreStatBuilderFromCoreBuilder<T>(
                _coreBuilder.Resolve(context), _statFactory, _entityBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new CoreStatBuilderFromCoreBuilder<T>(_coreBuilder, _statFactory, entityBuilder);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new CoreStatBuilderFromCoreBuilder<T>(_coreBuilder, _statFactory.AndThen(ss => ss.Select(statConverter)));

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource)
        {
            var stats = BuildStats(parameters).ToList();
            return new[] { new StatBuilderResult(stats, originalModifierSource, Funcs.Identity) };
        }

        private IEnumerable<IStat> BuildStats(BuildParameters parameters)
        {
            var entities = _entityBuilder.Build(parameters.ModifierSourceEntity);
            var t = _coreBuilder.Build();
            return entities.SelectMany(e => _statFactory(e, t));
        }
    }
}