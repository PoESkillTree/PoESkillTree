using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class DamageRelatedStatBuilder : StatBuilder, IDamageRelatedStatBuilder
    {
        private readonly DamageSpecificationBuilder _specificationBuilder;

        public DamageRelatedStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
            : this(statFactory, coreStatBuilder, new DamageSpecificationBuilder())
        {
        }

        private DamageRelatedStatBuilder(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder,
            DamageSpecificationBuilder specificationBuilder)
            : base(statFactory, coreStatBuilder)
        {
            _specificationBuilder = specificationBuilder;
        }

        protected override IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            new DamageRelatedStatBuilder(StatFactory, coreStatBuilder, _specificationBuilder);

        private IDamageRelatedStatBuilder With(DamageSpecificationBuilder specificationBuilder) =>
            new DamageRelatedStatBuilder(StatFactory, CoreStatBuilder, specificationBuilder);

        public override IStatBuilder Resolve(ResolveContext context) =>
            new DamageRelatedStatBuilder(StatFactory, CoreStatBuilder.Resolve(context),
                _specificationBuilder.Resolve(context));

        public IDamageRelatedStatBuilder With(DamageSource source) => With(_specificationBuilder.With(source));

        public IDamageRelatedStatBuilder WithHits => With(_specificationBuilder.WithHits());

        public IDamageRelatedStatBuilder WithHitsAndAilments => With(_specificationBuilder.WithHitsAndAilments());

        public IDamageRelatedStatBuilder WithAilments => With(_specificationBuilder.WithAilments());

        public IDamageRelatedStatBuilder With(IAilmentBuilder ailment) => With(_specificationBuilder.With(ailment));

        public IDamageRelatedStatBuilder WithSkills => With(_specificationBuilder.WithSkills());

        public IDamageRelatedStatBuilder With(AttackDamageHand hand) => With(_specificationBuilder.With(hand));

        public IStatBuilder ApplyModifiersTo(DamageSource source, params Form[] forms)
        {
            var coreStatBuilders = forms.Select(ConvertCore).ToList();
            return With(new CompositeCoreStatBuilder(coreStatBuilders));

            ICoreStatBuilder ConvertCore(Form form) =>
                CoreStatBuilder.WithStatConverter(s => StatFactory.ApplyModifiersToSkillDamage(s, source, form));
        }

        public IStatBuilder ApplyModifiersToAilments(params Form[] forms)
        {
            var coreStatBuilders = forms.Select(ConvertCore).ToList();
            return With(new CompositeCoreStatBuilder(coreStatBuilders));

            ICoreStatBuilder ConvertCore(Form form) =>
                CoreStatBuilder.WithStatConverter(s => StatFactory.ApplyModifiersToAilmentDamage(s, form));
        }

        public override IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            from baseResult in base.Build(parameters, originalModifierSource)
            from result in Concretize(baseResult)
            select result;

        private IEnumerable<StatBuilderResult> Concretize(StatBuilderResult result) =>
            from spec in _specificationBuilder.Build()
            let stats = result.Stats.Select(s => StatFactory.ConcretizeDamage(s, spec)).ToList()
            select new StatBuilderResult(stats, result.ModifierSource, result.ValueConverter);
    }
}