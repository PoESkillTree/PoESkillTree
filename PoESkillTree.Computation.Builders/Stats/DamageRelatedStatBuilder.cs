using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class DamageRelatedStatBuilder : StatBuilder, IDamageRelatedStatBuilder
    {
        private readonly DamageStatConcretizer _damageStatConcretizer;

        public DamageRelatedStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder)
            : base(statFactory, coreStatBuilder)
        {
            _damageStatConcretizer = new DamageStatConcretizer(statFactory);
        }

        protected override IFlagStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            new DamageRelatedStatBuilder(StatFactory, coreStatBuilder);

        public IDamageRelatedStatBuilder With(DamageSource source)
        {
            throw new System.NotImplementedException();
        }

        public IDamageRelatedStatBuilder WithHits { get; }
        public IDamageRelatedStatBuilder WithHitsAndAilments { get; }
        public IDamageRelatedStatBuilder WithAilments { get; }

        public IDamageRelatedStatBuilder With(IAilmentBuilder ailment)
        {
            throw new System.NotImplementedException();
        }

        public IDamageRelatedStatBuilder WithSkills { get; }

        public IDamageRelatedStatBuilder With(AttackDamageHand hand)
        {
            throw new System.NotImplementedException();
        }

        public IStatBuilder ApplyModifiersTo(DamageSource source, params Form[] forms)
        {
            throw new System.NotImplementedException();
        }

        public IStatBuilder ApplyModifiersToAilments(params Form[] forms)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<StatBuilderResult> Build(
            BuildParameters parameters, ModifierSource originalModifierSource) =>
            base.Build(parameters, originalModifierSource)
                .SelectMany(_damageStatConcretizer.Concretize);
    }
}