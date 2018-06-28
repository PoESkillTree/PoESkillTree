using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class DamageRelatedStatBuilder : StatBuilder, IDamageRelatedStatBuilder
    {
        private readonly DamageStatConcretizer _statConcretizer;
        private readonly Func<IStat, IEnumerable<IStat>> _statConverter;

        public static IDamageRelatedStatBuilder Create(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder,
            bool canApplyToSkillDamage = false, bool canApplyToAilmentDamage = false)
        {
            return new DamageRelatedStatBuilder(statFactory, coreStatBuilder,
                new DamageStatConcretizer(statFactory, new DamageSpecificationBuilder(), canApplyToSkillDamage,
                    canApplyToAilmentDamage),
                s => new[] { s });
        }

        private DamageRelatedStatBuilder(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder,
            DamageStatConcretizer statConcretizer,
            Func<IStat, IEnumerable<IStat>> statConverter)
            : base(statFactory, coreStatBuilder)
        {
            _statConcretizer = statConcretizer;
            _statConverter = statConverter;
        }

        protected override IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            new DamageRelatedStatBuilder(StatFactory, coreStatBuilder, _statConcretizer, _statConverter);

        private IDamageRelatedStatBuilder With(DamageStatConcretizer statConcretizer) =>
            new DamageRelatedStatBuilder(StatFactory, CoreStatBuilder, statConcretizer, _statConverter);

        protected override IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            With(s => new[] { statConverter(s) });

        private IStatBuilder With(Func<IStat, IEnumerable<IStat>> statConverter) =>
            new DamageRelatedStatBuilder(StatFactory, CoreStatBuilder, _statConcretizer.NotDamageRelated(),
                statConverter);

        public override IStatBuilder Resolve(ResolveContext context) =>
            new DamageRelatedStatBuilder(StatFactory, CoreStatBuilder.Resolve(context),
                _statConcretizer.Resolve(context), _statConverter);

        public IDamageRelatedStatBuilder With(DamageSource source) => With(_statConcretizer.With(source));

        public IDamageRelatedStatBuilder WithHits => With(_statConcretizer.WithHits());

        public IDamageRelatedStatBuilder WithHitsAndAilments => With(_statConcretizer.WithHitsAndAilments());

        public IDamageRelatedStatBuilder WithAilments => With(_statConcretizer.WithAilments());

        public IDamageRelatedStatBuilder With(IAilmentBuilder ailment) => With(_statConcretizer.With(ailment));

        public IDamageRelatedStatBuilder WithSkills => With(_statConcretizer.WithSkills());

        public IDamageRelatedStatBuilder With(AttackDamageHand hand) => With(_statConcretizer.With(hand));

        public IStatBuilder ApplyModifiersToSkills(DamageSource source, params Form[] forms)
        {
            if (!_statConcretizer.CanApplyToSkillDamage)
                throw new ParseException("Can't apply skill damage modifiers to this stat to other damage sources");
            return With(s => forms.Select(f => StatFactory.ApplyModifiersToSkillDamage(s, source, f)));
        }

        public IStatBuilder ApplyModifiersToAilments(params Form[] forms)
        {
            if (!_statConcretizer.CanApplyToAilmentDamage)
                throw new ParseException("Can't apply skill damage modifiers to this stat to ailments");
            return With(s => forms.Select(f => StatFactory.ApplyModifiersToAilmentDamage(s, f)));
        }

        public override IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            from baseResult in base.Build(parameters, originalModifierSource)
            from result in _statConcretizer.Concretize(parameters.ModifierForm, baseResult)
            select new StatBuilderResult(result.Stats.SelectMany(_statConverter).ToList(), result.ModifierSource,
                result.ValueConverter);
    }
}