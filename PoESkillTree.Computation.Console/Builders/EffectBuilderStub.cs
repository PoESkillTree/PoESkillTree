using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EffectBuilderStub : BuilderStub, IEffectBuilder
    {
        public EffectBuilderStub(string stringRepresentation) 
            : base(stringRepresentation)
        {
        }

        public IFlagStatBuilder On(IEntityBuilder target) =>
            new FlagStatBuilderStub($"Apply {this} to {target}");

        public IStatBuilder ChanceOn(IEntityBuilder target) =>
            new StatBuilderStub($"Chance to apply {this} to {target}");

        public IConditionBuilder IsOn(IEntityBuilder target) =>
            new ConditionBuilderStub($"{this} is applied to {target}");

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration");
    }


    public class EffectBuildersStub : IEffectBuilders
    {
        public IStunEffectBuilder Stun => new StunEffectBuilderStub();

        public IKnockbackEffectBuilder Knockback => new KnockbackEffectBuilderStub();

        public IAilmentBuilders Ailment => new AilmentBuildersStub();

        public IGroundEffectBuilders Ground => new GroundEffectBuildersStub();
    }


    public class AvoidableEffectBuilderStub : EffectBuilderStub, IAvoidableEffectBuilder
    {
        public AvoidableEffectBuilderStub(string stringRepresentation) 
            : base(stringRepresentation)
        {
        }

        public IStatBuilder Avoidance => new StatBuilderStub($"{this} avoidance");
    }


    public class KnockbackEffectBuilderStub : EffectBuilderStub, IKnockbackEffectBuilder
    {
        public KnockbackEffectBuilderStub() 
            : base("Knockback")
        {
        }

        public IStatBuilder Distance => new StatBuilderStub($"{this} distance");
    }
}