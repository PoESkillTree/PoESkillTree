using PoESkillTree.Common.Model.Items.Enums;
using PoESkillTree.Computation.Parsing.Builders.Buffs;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Effects;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class StatBuilderStub : BuilderStub, IStatBuilder
    {
        protected IConditionBuilders ConditionBuilders { get; }

        public StatBuilderStub(string stringRepresentation, IConditionBuilders conditionBuilders) 
            : base(stringRepresentation)
        {
            ConditionBuilders = conditionBuilders;
        }

        protected IStatBuilder Create(string stringRepresentation)
        {
            return new StatBuilderStub(stringRepresentation, ConditionBuilders);
        }

        public IStatBuilder Minimum => Create("Minimum" + this);
        public IStatBuilder Maximum => Create("Maximum " + this);

        public ValueBuilder Value =>
            new ValueBuilder(new ValueBuilderStub("Value of " + this), ConditionBuilders);

        public IStatBuilder ConvertTo(IStatBuilder stat) =>
            Create($"% of {this} converted to {stat}");

        public IStatBuilder AddAs(IStatBuilder stat) =>
            Create($"% of {this} added as {stat}");

        public IFlagStatBuilder ApplyModifiersTo(IStatBuilder stat,
            ValueBuilder percentOfTheirValue) =>
            new FlagStatBuilderStub(
                $"Modifiers to {this} apply to {stat} at {percentOfTheirValue}% of their value",
                ConditionBuilders);

        public IStatBuilder ChanceToDouble =>
            Create($"Chance to double {this}");

        public IBuffBuilder ForXSeconds(ValueBuilder seconds) =>
            new BuffBuilderStub($"{this} as Buff for {seconds} seconds", ConditionBuilders);

        public IBuffBuilder AsBuff =>
            new BuffBuilderStub($"{this} as Buff", ConditionBuilders);

        public IFlagStatBuilder AsAura(params IEntityBuilder[] affectedEntities) =>
            new FlagStatBuilderStub(
                $"{this} as Aura affecting [{string.Join<IEntityBuilder>(", ", affectedEntities)}]",
                ConditionBuilders);

        public IFlagStatBuilder AddTo(ISkillBuilderCollection skills) =>
            new FlagStatBuilderStub($"{this} added to skills {skills}", ConditionBuilders);

        public IFlagStatBuilder AddTo(IEffectBuilder effect) =>
            new FlagStatBuilderStub($"{this} added to effect {effect}", ConditionBuilders);
    }


    public class DamageStatBuilderStub : StatBuilderStub, IDamageStatBuilder
    {
        public DamageStatBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Taken => Create($"{this} taken");

        public IDamageTakenConversionBuilder TakenFrom(IPoolStatBuilder pool) =>
            new DamageTakenConversionBuilder($"{this} taken from {pool}", ConditionBuilders);

        public IConditionBuilder With() =>
            new ConditionBuilderStub($"With {this}");

        public IConditionBuilder With(IDamageSourceBuilder source) =>
            new ConditionBuilderStub($"With {source} {this}");

        public IConditionBuilder With(Tags tags) =>
            new ConditionBuilderStub($"With {tags} {this}");

        public IConditionBuilder With(IAilmentBuilder ailment) =>
            new ConditionBuilderStub($"With {ailment} {this}");

        public IConditionBuilder With(ItemSlot slot) =>
            new ConditionBuilderStub($"With {slot} {this}");


        private class DamageTakenConversionBuilder : BuilderStub, IDamageTakenConversionBuilder
        {
            private readonly IConditionBuilders _conditionBuilders;

            public DamageTakenConversionBuilder(string stringRepresentation,
                IConditionBuilders conditionBuilders) 
                : base(stringRepresentation)
            {
                _conditionBuilders = conditionBuilders;
            }

            public IStatBuilder Before(IPoolStatBuilder pool) 
                => new StatBuilderStub($"{this} before {pool}", _conditionBuilders);
        }
    }


    public class EvasionStatBuilderStub : StatBuilderStub, IEvasionStatBuilder
    {
        public EvasionStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Chance => Create($"Chance to {this}");

        public IStatBuilder ChanceAgainstProjectileAttacks =>
            Create($"Chance to {this} against projectile attacks");

        public IStatBuilder ChanceAgainstMeleeAttacks =>
            Create($"Chance to {this} against melee attacks");
    }
}