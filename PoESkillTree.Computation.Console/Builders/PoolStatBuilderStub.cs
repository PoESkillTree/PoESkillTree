using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class PoolStatBuilderStub : StatBuilderStub, IPoolStatBuilder
    {
        public PoolStatBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IRegenStatBuilder Regen =>
            new RegenStatBuilderStub($"{this} regeneration");

        public IRechargeStatBuilder Recharge =>
            new RechargeStatBuilderStub($"{this} recharge");

        public IStatBuilder RecoveryRate => Create($"{this} recovery rate");
        public IStatBuilder Reservation => Create($"{this} reservation");

        public ILeechStatBuilder Leech =>
            new LeechStatBuilderStub($"{this} Leech");

        public IFlagStatBuilder InstantLeech =>
            new FlagStatBuilderStub($"{this} gained from Leech instantly");

        public IStatBuilder Gain => Create($"{this} gain");

        public IConditionBuilder IsFull => new ConditionBuilderStub($"{this} is full");
        public IConditionBuilder IsLow => new ConditionBuilderStub($"{this} is low");
    }


    public class PoolStatBuildersStub : IPoolStatBuilders
    {
        private static IPoolStatBuilder Create(string stringRepresentation)
        {
            return new PoolStatBuilderStub(stringRepresentation);
        }

        public IPoolStatBuilder Life => Create("Life");
        public IPoolStatBuilder Mana => Create("Mana");
        public IPoolStatBuilder EnergyShield => Create("Energy Shield");
    }


    public class RechargeStatBuilderStub : StatBuilderStub, IRechargeStatBuilder
    {
        public RechargeStatBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Start => Create($"Start of {this}");

        public IConditionBuilder StartedRecently =>
            new ConditionBuilderStub($"{this} started recently");
    }


    public class RegenStatBuilderStub : StatBuilderStub, IRegenStatBuilder
    {
        public RegenStatBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Percent => Create($"Percent {this}");

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            new FlagStatBuilderStub($"{this} applies to {stat} instead");
    }


    public class LeechStatBuilderStub : BuilderStub, ILeechStatBuilder
    {
        public LeechStatBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Of(IDamageStatBuilder damage) =>
            new StatBuilderStub($"{damage} {this}");

        public IStatBuilder RateLimit =>
            new StatBuilderStub($"Maximum {this} rate per second");
        public IStatBuilder Rate =>
            new StatBuilderStub($"{this} per second");

        public IFlagStatBuilder AppliesTo(IPoolStatBuilder stat) =>
            new FlagStatBuilderStub($"{this} applies to {stat} instead");

        public ILeechStatBuilder To(IEntityBuilder entity) =>
            new LeechStatBuilderStub($"{this} leeched to {entity}");

        public IFlagStatBuilder BasedOn(IDamageTypeBuilder damageType) =>
            new FlagStatBuilderStub($"{this} recovers based on {damageType} instead");
    }
}