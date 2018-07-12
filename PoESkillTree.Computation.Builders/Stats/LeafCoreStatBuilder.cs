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
    public class LeafCoreStatBuilder : ICoreStatBuilder
    {
        public delegate IStat StatFactory(BuildParameters parameters, Entity entity);

        private readonly StatFactory _statFactory;
        private readonly IEntityBuilder _entityBuilder;

        public LeafCoreStatBuilder(Func<Entity, IStat> statFactory, IEntityBuilder entityBuilder = null)
            : this((ps, e) => statFactory(e), entityBuilder)
        {
        }

        public LeafCoreStatBuilder(StatFactory statFactory, IEntityBuilder entityBuilder = null)
        {
            _statFactory = statFactory;
            _entityBuilder = entityBuilder ?? new ModifierSourceEntityBuilder();
        }

        public static ICoreStatBuilder FromIdentity(
            IStatFactory statFactory, string identity, Type dataType,
            ExplicitRegistrationType explicitRegistrationType = null) =>
            new LeafCoreStatBuilder(
                entity => statFactory.FromIdentity(identity, entity, dataType, explicitRegistrationType));

        public ICoreStatBuilder Resolve(ResolveContext context) => WithEntity(_entityBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new LeafCoreStatBuilder(_statFactory, entityBuilder);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource)
        {
            var stats = BuildStats(parameters);
            return new[] { new StatBuilderResult(stats, originalModifierSource, Funcs.Identity) };
        }

        private IReadOnlyList<IStat> BuildStats(BuildParameters parameters)
        {
            var entities = _entityBuilder.Build(parameters.ModifierSourceEntity);
            return entities.Select(e => _statFactory(parameters, e)).ToList();
        }
    }
}