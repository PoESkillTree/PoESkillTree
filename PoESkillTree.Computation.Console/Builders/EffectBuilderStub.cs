using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EffectBuilderStub : BuilderStub, IEffectBuilder
    {
        private readonly Resolver<IEffectBuilder> _resolver;

        public EffectBuilderStub(string stringRepresentation, Resolver<IEffectBuilder> resolver) 
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        protected IEffectBuilder This => this;

        public IFlagStatBuilder On(IEntityBuilder target) =>
            CreateFlagStat(This, target, (o1, o2) => $"Apply {o1} to {o2}");

        public IStatBuilder ChanceOn(IEntityBuilder target) =>
            CreateStat(This, target, (o1, o2) => $"Chance to apply {o1} to {o2}");

        public IConditionBuilder IsOn(IEntityBuilder target) =>
            CreateCondition(This, target, (l, r) => $"{l} is applied to {r}");

        public IStatBuilder Duration =>
            CreateStat(This, o => $"{o} duration");

        public IStatBuilder AddStat(IStatBuilder stat) =>
            CreateStat(This, stat, (o1, o2) => $"{o2} added to effect {o1}");

        public IEffectBuilder Resolve(ResolveContext context) =>
            _resolver(this, context);
    }


    public class EffectBuildersStub : IEffectBuilders
    {
        public IStunEffectBuilder Stun => new StunEffectBuilderStub();

        public IKnockbackEffectBuilder Knockback => new KnockbackEffectBuilderStub();

        public IAilmentBuilders Ailment => new AilmentBuildersStub();

        public IGroundEffectBuilders Ground => new GroundEffectBuildersStub();
    }


    public abstract class AvoidableEffectBuilderStub : EffectBuilderStub, IAvoidableEffectBuilder
    {
        protected AvoidableEffectBuilderStub(string stringRepresentation, Resolver<IEffectBuilder> resolver) 
            : base(stringRepresentation, resolver)
        {
        }

        public IStatBuilder Avoidance => CreateStat(This, o => $"{o} avoidance");
    }


    public class KnockbackEffectBuilderStub : EffectBuilderStub, IKnockbackEffectBuilder
    {
        public KnockbackEffectBuilderStub() 
            : base("Knockback", (current, _) => current)
        {
        }

        public IStatBuilder Distance => CreateStat(This, o => $"{o} distance");
    }
}