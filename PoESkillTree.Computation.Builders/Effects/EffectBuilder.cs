using System;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
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

        public virtual IFlagStatBuilder On(IEntityBuilder target) =>
            InternalOn(target);

        protected IFlagStatBuilder InternalOn(IEntityBuilder target) =>
            (IFlagStatBuilder) FromIdentity("Active", typeof(bool)).For(target);

        public IDamageRelatedStatBuilder Chance =>
            DamageRelatedFromIdentity("ChanceToActivate", typeof(int)).WithHits;

        public IConditionBuilder IsOn(IEntityBuilder target) => InternalOn(target).IsSet;

        public virtual IStatBuilder Duration =>
            FromIdentity("Duration", typeof(double));

        public virtual IStatBuilder AddStat(IStatBuilder stat) =>
            stat.WithCondition(IsOn(new ModifierSourceEntityBuilder()));

        protected IStatBuilder FromIdentity(string identitySuffx, Type dataType) =>
            new StatBuilder(StatFactory, CoreStatBuilderFromIdentity(identitySuffx, dataType));

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(string identitySuffx, Type dataType) =>
            DamageRelatedStatBuilder.Create(StatFactory, CoreStatBuilderFromIdentity(identitySuffx, dataType));

        protected ICoreStatBuilder CoreStatBuilderFromIdentity(string identitySuffx, Type dataType) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity,
                (e, id) => StatFactory.FromIdentity($"{id}.{identitySuffx}", e, dataType));

        public string Build() => Identity.Build();
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
            FromIdentity("ChanceToAvoid", typeof(int));
    }

    internal class GroundEffectBuilders : IGroundEffectBuilders
    {
        public GroundEffectBuilders(IStatFactory statFactory)
        {
            Consecrated = new EffectBuilder(statFactory, CoreBuilder.Create("ConsecratedGround"));
        }

        public IEffectBuilder Consecrated { get; }
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
            FromIdentity("ChanceToAvoidInterruptionWhileCasting", typeof(int));
    }
}