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
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class LeafCoreStatBuilder : ICoreStatBuilder
    {
        private readonly Func<Entity, IStat> _statFactory;
        private readonly IEntityBuilder _entityBuilder;

        public LeafCoreStatBuilder(Func<Entity, IStat> statFactory, IEntityBuilder entityBuilder = null)
        {
            _statFactory = statFactory;
            _entityBuilder = entityBuilder ?? new ModifierSourceEntityBuilder();
        }

        public static ICoreStatBuilder FromIdentity(
            IStatFactory statFactory, string identity, Type dataType, bool isExplicitlyRegistered = false) =>
            new LeafCoreStatBuilder(
                entity => statFactory.FromIdentity(identity, entity, dataType, isExplicitlyRegistered));

        public ICoreStatBuilder Resolve(ResolveContext context) => WithEntity(_entityBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new LeafCoreStatBuilder(_statFactory, entityBuilder);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new LeafCoreStatBuilder(_statFactory.AndThen(statConverter), _entityBuilder);

        public IValue BuildValue(BuildParameters parameters)
        {
            var stats = BuildStats(parameters);
            if (stats.Count != 1)
                throw new ParseException("Can only access the value of stat builders that represent a single stat");

            return new StatValue(stats.Single());
        }

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource)
        {
            var stats = BuildStats(parameters);
            return new[] { new StatBuilderResult(stats, originalModifierSource, Funcs.Identity) };
        }

        private IReadOnlyList<IStat> BuildStats(BuildParameters parameters)
        {
            var entities = _entityBuilder.Build(parameters.ModifierSourceEntity);
            return entities.Select(_statFactory).ToList();
        }
    }
}