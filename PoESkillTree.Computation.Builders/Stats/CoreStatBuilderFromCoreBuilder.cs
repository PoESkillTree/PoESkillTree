using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Builders.Stats
{
    internal class CoreStatBuilderFromCoreBuilder<T> : ICoreStatBuilder
    {
        public delegate IEnumerable<IStat> StatFactory(BuildParameters parameters, Entity entity, T coreResult);

        private readonly ICoreBuilder<T> _coreBuilder;
        private readonly StatFactory _statFactory;
        private readonly IEntityBuilder _entityBuilder;

        public CoreStatBuilderFromCoreBuilder(
            ICoreBuilder<T> coreBuilder, Func<Entity, T, IStat> statFactory, IEntityBuilder entityBuilder = null)
            : this(coreBuilder, (ps, e, t) => new[] { statFactory(e, t) }, entityBuilder)
        {
        }

        public CoreStatBuilderFromCoreBuilder(
            ICoreBuilder<T> coreBuilder, Func<BuildParameters, Entity, T, IStat> statFactory,
            IEntityBuilder entityBuilder = null)
            : this(coreBuilder, (ps, e, t) => new[] { statFactory(ps, e, t) }, entityBuilder)
        {
        }

        public CoreStatBuilderFromCoreBuilder(
            ICoreBuilder<T> coreBuilder,
            StatFactory statFactory,
            IEntityBuilder entityBuilder = null)
        {
            _coreBuilder = coreBuilder;
            _statFactory = statFactory;
            _entityBuilder = entityBuilder ?? new ModifierSourceEntityBuilder();
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new CoreStatBuilderFromCoreBuilder<T>(_coreBuilder.Resolve(context), _statFactory, _entityBuilder);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new CoreStatBuilderFromCoreBuilder<T>(_coreBuilder, _statFactory, entityBuilder);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
        {
            var stats = BuildStats(parameters).ToList();
            return new[] { new StatBuilderResult(stats, parameters.ModifierSource, Funcs.Identity) };
        }

        private IEnumerable<IStat> BuildStats(BuildParameters parameters)
        {
            var entities = _entityBuilder.Build(parameters.ModifierSourceEntity);
            var t = _coreBuilder.Build();
            return entities.SelectMany(e => _statFactory(parameters, e, t));
        }
    }
}