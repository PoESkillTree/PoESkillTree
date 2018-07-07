using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Damage
{
    internal class DamageTypeCoreStatBuilder : ICoreStatBuilder
    {
        private readonly Func<Entity, DamageType, IStat> _statFactory;
        private readonly ICoreBuilder<IEnumerable<DamageType>> _damageType;
        private readonly IEntityBuilder _entityBuilder;

        public DamageTypeCoreStatBuilder(
            Func<Entity, DamageType, IStat> statFactory, ICoreBuilder<IEnumerable<DamageType>> damageType,
            IEntityBuilder entityBuilder)
        {
            _statFactory = statFactory;
            _damageType = damageType;
            _entityBuilder = entityBuilder;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new DamageTypeCoreStatBuilder(_statFactory, _damageType.Resolve(context), _entityBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new DamageTypeCoreStatBuilder(_statFactory, _damageType, entityBuilder);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new DamageTypeCoreStatBuilder(_statFactory.AndThen(statConverter), _damageType, _entityBuilder);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource)
        {
            var stats = BuildStats(parameters).ToList();
            return new[] { new StatBuilderResult(stats, originalModifierSource, Funcs.Identity) };
        }

        private IEnumerable<IStat> BuildStats(BuildParameters parameters) =>
            from entity in _entityBuilder.Build(parameters.ModifierSourceEntity)
            from damageType in _damageType.Build()
            select _statFactory(entity, damageType);
    }
}