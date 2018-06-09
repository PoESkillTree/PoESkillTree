using System;
using System.Linq;
using PoESkillTree.Common.Utils;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilderAdapter : ICoreStatBuilder
    {
        private readonly IStatBuilder _statBuilder;
        private readonly Func<IStat, IStat> _statConverter;

        public StatBuilderAdapter(IStatBuilder statBuilder) : this(statBuilder, Funcs.Identity)
        {
        }

        private StatBuilderAdapter(IStatBuilder statBuilder, Func<IStat, IStat> statConverter)
        {
            _statBuilder = statBuilder;
            _statConverter = statConverter;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new StatBuilderAdapter(_statBuilder.Resolve(context), _statConverter);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new StatBuilderAdapter(_statBuilder.For(entityBuilder), _statConverter);

        public ICoreStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            new StatBuilderAdapter(_statBuilder, statConverter);

        public ICoreStatBuilder CombineWith(ICoreStatBuilder other) =>
            new CompositeCoreStatBuilder(this, other);

        public IValue BuildValue(Entity modifierSourceEntity) =>
            _statBuilder.Value.Build(modifierSourceEntity);

        public StatBuilderResult Build(ModifierSource originalModifierSource, Entity modifierSourceEntity)
        {
            var (stats, modifierSource, valueConverter) =
                _statBuilder.Build(originalModifierSource, modifierSourceEntity);
            stats = stats.Select(_statConverter).ToList();
            return new StatBuilderResult(stats, modifierSource, valueConverter);
        }
    }
}