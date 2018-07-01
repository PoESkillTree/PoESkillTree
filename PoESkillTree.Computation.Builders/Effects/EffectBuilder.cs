using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Builders.Effects
{
    public class EffectBuilder : StatBuildersBase, IEffectBuilder
    {
        protected string Identity { get; }

        public EffectBuilder(IStatFactory statFactory, string identity) : base(statFactory)
        {
            Identity = identity;
        }

        public IEffectBuilder Resolve(ResolveContext context) => this;

        public IFlagStatBuilder On(IEntityBuilder target) =>
            (IFlagStatBuilder) FromIdentity($"{Identity}.Active", typeof(bool)).For(target);

        public IStatBuilder ChanceOn(IEntityBuilder target) =>
            FromIdentity($"{Identity}.ChanceToBecomeActive", typeof(int)).For(target);

        public IConditionBuilder IsOn(IEntityBuilder target) => On(target).IsSet;

        public IStatBuilder Duration =>
            FromIdentity($"{Identity}.Duration", typeof(double));

        public IStatBuilder AddStat(IStatBuilder stat) =>
            stat.WithCondition(IsOn(new ModifierSourceEntityBuilder()));
    }

    public class KnockbackEffectBuilder : EffectBuilder, IKnockbackEffectBuilder
    {
        public KnockbackEffectBuilder(IStatFactory statFactory) : base(statFactory, "Knockback")
        {
        }

        public IStatBuilder Distance =>
            FromIdentity($"{Identity}.Distance", typeof(int));
    }

    public class AvoidableEffectBuilder : EffectBuilder, IAvoidableEffectBuilder
    {
        public AvoidableEffectBuilder(IStatFactory statFactory, string identity) : base(statFactory, identity)
        {
        }

        public IStatBuilder Avoidance =>
            FromIdentity($"{Identity}.ChanceToAvoid", typeof(int));
    }

    public class GroundEffectBuilders : IGroundEffectBuilders
    {
        public GroundEffectBuilders(IStatFactory statFactory)
        {
            Consecrated = new EffectBuilder(statFactory, "ConsecratedGround");
        }

        public IEffectBuilder Consecrated { get; }
    }
}