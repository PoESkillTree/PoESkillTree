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
        protected DamageStatConcretizer StatConcretizer { get; }
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

        protected DamageRelatedStatBuilder(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder,
            DamageStatConcretizer statConcretizer,
            Func<IStat, IEnumerable<IStat>> statConverter)
            : base(statFactory, coreStatBuilder)
        {
            StatConcretizer = statConcretizer;
            _statConverter = statConverter;
        }

        protected virtual DamageRelatedStatBuilder Create(
            ICoreStatBuilder coreStatBuilder,
            DamageStatConcretizer statConcretizer,
            Func<IStat, IEnumerable<IStat>> statConverter) =>
            new DamageRelatedStatBuilder(StatFactory, coreStatBuilder, statConcretizer, statConverter);

        protected override IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            Create(coreStatBuilder, StatConcretizer, _statConverter);

        protected IDamageRelatedStatBuilder With(DamageStatConcretizer statConcretizer) =>
            Create(CoreStatBuilder, statConcretizer, _statConverter);

        protected override IStatBuilder WithStatConverter(Func<IStat, IStat> statConverter) =>
            With(s => new[] { statConverter(s) });

        private IStatBuilder With(Func<IStat, IEnumerable<IStat>> statConverter) =>
            Create(CoreStatBuilder, StatConcretizer.NotDamageRelated(), statConverter);

        public override IStatBuilder Resolve(ResolveContext context) =>
            Create(CoreStatBuilder.Resolve(context), StatConcretizer.Resolve(context), _statConverter);

        public IDamageRelatedStatBuilder With(DamageSource source) => With(StatConcretizer.With(source));

        public IDamageRelatedStatBuilder WithHits => With(StatConcretizer.WithHits());

        public IDamageRelatedStatBuilder WithHitsAndAilments => With(StatConcretizer.WithHitsAndAilments());

        public IDamageRelatedStatBuilder WithAilments => With(StatConcretizer.WithAilments());

        public IDamageRelatedStatBuilder With(IAilmentBuilder ailment) => With(StatConcretizer.With(ailment));

        public IDamageRelatedStatBuilder WithSkills => With(StatConcretizer.WithSkills());

        public IDamageRelatedStatBuilder With(AttackDamageHand hand) => With(StatConcretizer.With(hand));

        public IStatBuilder ApplyModifiersToSkills(DamageSource source, params Form[] forms)
        {
            if (!StatConcretizer.CanApplyToSkillDamage)
                throw new ParseException("Can't apply skill damage modifiers to this stat to other damage sources");
            return With(s => forms.Select(f => StatFactory.ApplyModifiersToSkillDamage(s, source, f)));
        }

        public IStatBuilder ApplyModifiersToAilments(params Form[] forms)
        {
            if (!StatConcretizer.CanApplyToAilmentDamage)
                throw new ParseException("Can't apply skill damage modifiers to this stat to ailments");
            return With(s => forms.Select(f => StatFactory.ApplyModifiersToAilmentDamage(s, f)));
        }

        public override IEnumerable<StatBuilderResult>
            Build(BuildParameters parameters, ModifierSource originalModifierSource) =>
            from baseResult in base.Build(parameters, originalModifierSource)
            from result in StatConcretizer.Concretize(parameters, baseResult)
            select new StatBuilderResult(result.Stats.SelectMany(_statConverter).ToList(), result.ModifierSource,
                result.ValueConverter);
    }
}