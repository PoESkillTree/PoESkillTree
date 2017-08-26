using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class EffectBuilderStub : BuilderStub, IEffectBuilder
    {
        protected IConditionBuilders ConditionBuilders { get; }

        public EffectBuilderStub(string stringRepresentation, IConditionBuilders conditionBuilders) 
            : base(stringRepresentation)
        {
            ConditionBuilders = conditionBuilders;
        }

        public IFlagStatBuilder On(IEntityBuilder target) =>
            new FlagStatBuilderStub($"Apply {this} to {target}", ConditionBuilders);

        public IStatBuilder ChanceOn(IEntityBuilder target) =>
            new StatBuilderStub($"Chance to apply {this} to {target}", ConditionBuilders);

        public IConditionBuilder IsOn(IEntityBuilder target) =>
            new ConditionBuilderStub($"{this} is applied to {target}");

        public IStatBuilder Duration =>
            new StatBuilderStub($"{this} duration", ConditionBuilders);
    }


    public class EffectBuildersStub : IEffectBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public EffectBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IStunEffectBuilder Stun => new StunEffectBuilderStub(_conditionBuilders);

        public IKnockbackEffectBuilder Knockback =>
            new KnockbackEffectBuilderStub(_conditionBuilders);

        public IAilmentBuilders Ailment => new AilmentBuildersStub(_conditionBuilders);

        public IGroundEffectBuilders Ground => new GroundEffectBuildersStub(_conditionBuilders);
    }


    public class AvoidableEffectBuilderStub : EffectBuilderStub, IAvoidableEffectBuilder
    {
        public AvoidableEffectBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Avoidance =>
            new StatBuilderStub($"{this} avoidance", ConditionBuilders);
    }


    public class KnockbackEffectBuilderStub : EffectBuilderStub, IKnockbackEffectBuilder
    {
        public KnockbackEffectBuilderStub(IConditionBuilders conditionBuilders) 
            : base("Knockback", conditionBuilders)
        {
        }

        public IStatBuilder Distance => new StatBuilderStub($"{this} distance", ConditionBuilders);
    }
}