using System;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Effects
{
    public class EffectBuilder : IEffectBuilder
    {
        protected IStatFactory StatFactory { get; }
        protected ICoreBuilder<string> Identity { get; }

        public EffectBuilder(IStatFactory statFactory, ICoreBuilder<string> identity)
        {
            StatFactory = statFactory;
            Identity = identity;
        }

        public virtual IEffectBuilder Resolve(ResolveContext context) =>
            new EffectBuilder(StatFactory, Identity.Resolve(context));

        public virtual IStatBuilder On(IEntityBuilder target) =>
            InternalOn(target);

        protected IStatBuilder InternalOn(IEntityBuilder target)
            => FromIdentity("Active", typeof(bool),
                    OnIsUserSpecified ? ExplicitRegistrationTypes.UserSpecifiedValue(false) : null)
                .For(target);

        protected virtual bool OnIsUserSpecified => false;

        public IDamageRelatedStatBuilder Chance =>
            DamageRelatedFromIdentity("ChanceToActivate", typeof(int)).WithHits;

        public IConditionBuilder IsOn(IEntityBuilder target) => InternalOn(target).IsSet;

        public virtual IStatBuilder Duration =>
            FromIdentity("Duration", typeof(double));

        public virtual IStatBuilder AddStat(IStatBuilder stat) =>
            stat.WithCondition(IsOn(new ModifierSourceEntityBuilder()));

        protected IStatBuilder FromIdentity(
            string identitySuffix, Type dataType, ExplicitRegistrationType explicitRegistrationType = null)
            => new StatBuilder(StatFactory,
                CoreStatBuilderFromIdentity(identitySuffix, dataType, explicitRegistrationType));

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(string identitySuffix, Type dataType) =>
            DamageRelatedStatBuilder.Create(StatFactory, CoreStatBuilderFromIdentity(identitySuffix, dataType));

        protected ICoreStatBuilder CoreStatBuilderFromIdentity(
            string identitySuffix, Type dataType, ExplicitRegistrationType explicitRegistrationType = null)
            => new CoreStatBuilderFromCoreBuilder<string>(Identity,
                (e, id) => StatFactory.FromIdentity($"{id}.{identitySuffix}", e, dataType, explicitRegistrationType));

        public string Build(BuildParameters parameters) => Identity.Build(parameters);
    }

    internal class KnockbackEffectBuilder : EffectBuilder, IKnockbackEffectBuilder
    {
        public KnockbackEffectBuilder(IStatFactory statFactory)
            : base(statFactory, CoreBuilder.Create("Knockback"))
        {
        }

        public IStatBuilder Distance =>
            FromIdentity("Distance", typeof(int));
    }

    public class AvoidableEffectBuilder : EffectBuilder, IAvoidableEffectBuilder
    {
        public AvoidableEffectBuilder(IStatFactory statFactory, ICoreBuilder<string> identity) : base(statFactory, identity)
        {
        }

        public IStatBuilder Avoidance =>
            FromIdentity("ChanceToAvoid", typeof(uint));
    }

    internal class GroundEffectBuilders : IGroundEffectBuilders
    {
        public GroundEffectBuilders(IStatFactory statFactory)
        {
            Consecrated = new GroundEffectBuilder(statFactory);
        }

        public IEffectBuilder Consecrated { get; }

        private class GroundEffectBuilder : EffectBuilder
        {
            public GroundEffectBuilder(IStatFactory statFactory)
                : base(statFactory, CoreBuilder.Create("ConsecratedGround"))
            {
            }

            protected override bool OnIsUserSpecified => true;
        }
    }

    internal class StunEffectBuilder : AvoidableEffectBuilder, IStunEffectBuilder
    {
        public StunEffectBuilder(IStatFactory statFactory)
            : base(statFactory, CoreBuilder.Create("Stun"))
        {
        }

        public override IStatBuilder Duration =>
            DamageRelatedFromIdentity("Duration", typeof(double)).WithHits;

        public IDamageRelatedStatBuilder Threshold =>
            DamageRelatedFromIdentity("ThresholdModifier", typeof(double)).WithHits;

        public IStatBuilder Recovery => FromIdentity("RecoveryModifier", typeof(double));

        public IStatBuilder ChanceToAvoidInterruptionWhileCasting =>
            FromIdentity("ChanceToAvoidInterruptionWhileCasting", typeof(uint));
    }
}