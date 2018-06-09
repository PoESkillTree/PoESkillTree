using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class LeafCoreStatBuilder : ICoreStatBuilder
    {
        private readonly string _identity;
        private readonly IEntityBuilder _entityBuilder;
        private readonly bool _isRegisteredExplicitly;
        private readonly Type _dataType;
        private readonly IReadOnlyCollection<Behavior> _behaviors;
        private readonly Func<IStat, IStat> _statConverter;

        public LeafCoreStatBuilder(
            string identity, IEntityBuilder entityBuilder, bool isRegisteredExplicitly = false, Type dataType = null,
            IReadOnlyCollection<Behavior> behaviors = null)
            : this(identity, entityBuilder, isRegisteredExplicitly, dataType, behaviors, Funcs.Identity)
        {
        }

        private LeafCoreStatBuilder(
            string identity, IEntityBuilder entityBuilder, bool isRegisteredExplicitly, Type dataType,
            IReadOnlyCollection<Behavior> behaviors, Func<IStat, IStat> statConverter)
        {
            _identity = identity;
            _entityBuilder = entityBuilder;
            _isRegisteredExplicitly = isRegisteredExplicitly;
            _dataType = dataType;
            _behaviors = behaviors;
            _statConverter = statConverter;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) => WithEntity(_entityBuilder.Resolve(context));

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new LeafCoreStatBuilder(_identity, entityBuilder, _isRegisteredExplicitly, _dataType, _behaviors,
                _statConverter);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new LeafCoreStatBuilder(_identity, _entityBuilder, _isRegisteredExplicitly, _dataType, _behaviors,
                s => statConverter(_statConverter(s)));

        public ICoreStatBuilder CombineWith(ICoreStatBuilder other) =>
            new CompositeCoreStatBuilder(this, other);

        public IValue BuildValue(Entity modifierSourceEntity)
        {
            var stats = BuildStats(modifierSourceEntity);
            if (stats.Count != 1)
                throw new InvalidOperationException(
                    "Can only access the value of IStatBuilders that represent a single stat");

            return new FunctionalValue(c => c.GetValue(stats.Single()), $"{_identity}.Value");
        }

        public StatBuilderResult Build(ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var stats = BuildStats(modifierSourceEntity);
            return new StatBuilderResult(stats, originalModifierSource, Funcs.Identity);
        }

        private IReadOnlyList<IStat> BuildStats(Entity modifierSourceEntity)
        {
            var entities = _entityBuilder.Build(modifierSourceEntity).DefaultIfEmpty(modifierSourceEntity);
            return entities.Select(CreateStat).Select(_statConverter).ToList();
        }

        private IStat CreateStat(Entity modifierSourceEntity) =>
            new Stat(_identity, modifierSourceEntity, _isRegisteredExplicitly, _dataType, _behaviors);
    }
}