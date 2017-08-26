using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class PoolStatBuilderStub : StatBuilderStub, IPoolStatBuilder
    {
        public PoolStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IRegenStatBuilder Regen =>
            new RegenStatBuilderStub($"{this} regeneration", ConditionBuilders);

        public IRechargeStatBuilder Recharge =>
            new RechargeStatBuilderStub($"{this} recharge", ConditionBuilders);

        public IStatBuilder RecoveryRate => Create($"{this} recovery rate");
        public IStatBuilder Reservation => Create($"{this} reservation");

        public ILeechStatBuilder Leech =>
            new LeechStatBuilderStub($"{this} Leech", ConditionBuilders);

        public IFlagStatBuilder InstantLeech =>
            new FlagStatBuilderStub($"{this} gained from Leech instantly", ConditionBuilders);

        public IStatBuilder Gain => Create($"{this} gain");

        public IConditionBuilder IsFull => new ConditionBuilderStub($"{this} is full");
        public IConditionBuilder IsLow => new ConditionBuilderStub($"{this} is low");
    }


    public class PoolStatBuildersStub : IPoolStatBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public PoolStatBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        private IPoolStatBuilder Create(string stringRepresentation)
        {
            return new PoolStatBuilderStub(stringRepresentation, _conditionBuilders);
        }

        public IPoolStatBuilder Life => Create("Life");
        public IPoolStatBuilder Mana => Create("Mana");
        public IPoolStatBuilder EnergyShield => Create("Energy Shield");
    }


    public class RechargeStatBuilderStub : StatBuilderStub, IRechargeStatBuilder
    {
        public RechargeStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Start => Create($"Start of {this}");

        public IConditionBuilder StartedRecently =>
            new ConditionBuilderStub($"{this} started recently");
    }


    public class RegenStatBuilderStub : StatBuilderStub, IRegenStatBuilder
    {
        public RegenStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation, conditionBuilders)
        {
        }

        public IStatBuilder Percent => Create($"Percent {this}");

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            new FlagStatBuilderStub($"{this} applies to {stat} instead", ConditionBuilders);
    }


    public class LeechStatBuilderStub : BuilderStub, ILeechStatBuilder
    {
        private readonly IConditionBuilders _conditionBuilders;

        public LeechStatBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IStatBuilder Of(IDamageStatBuilder damage) =>
            new StatBuilderStub($"{damage} {this}", _conditionBuilders);

        public IStatBuilder RateLimit =>
            new StatBuilderStub($"Maximum {this} rate per second", _conditionBuilders);
        public IStatBuilder Rate =>
            new StatBuilderStub($"{this} per second", _conditionBuilders);

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            new FlagStatBuilderStub($"{this} applies to {stat} instead", _conditionBuilders);

        public ILeechStatBuilder To(IEntityBuilder entity) =>
            new LeechStatBuilderStub($"{this} leeched to {entity}", _conditionBuilders);

        public IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType) =>
            new FlagStatBuilderStub($"{this} recovers based on {damageType} instead",
                _conditionBuilders);
    }
}