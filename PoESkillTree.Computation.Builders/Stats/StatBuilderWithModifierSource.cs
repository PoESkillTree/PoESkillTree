using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatBuilderWithModifierSource : ICoreStatBuilder
    {
        private readonly ICoreStatBuilder _statBuilder;
        private readonly ModifierSource _modifierSource;

        public StatBuilderWithModifierSource(ICoreStatBuilder statBuilder, ModifierSource modifierSource)
        {
            _statBuilder = statBuilder;
            _modifierSource = modifierSource;
        }

        public ICoreStatBuilder Resolve(ResolveContext context) =>
            new StatBuilderWithModifierSource(_statBuilder.Resolve(context), _modifierSource);

        public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder) =>
            new StatBuilderWithModifierSource(_statBuilder.WithEntity(entityBuilder), _modifierSource);

        public IEnumerable<StatBuilderResult> Build(BuildParameters parameters) =>
            _statBuilder.Build(parameters.With(_modifierSource));
    }
}