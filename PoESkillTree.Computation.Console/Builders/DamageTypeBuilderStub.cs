using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageTypeBuilderStub : BuilderStub, IDamageTypeBuilder
    {
        public DamageTypeBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IDamageTypeBuilder And(IDamageTypeBuilder type) =>
            new DamageTypeBuilderStub($"{this}, {type}");

        public IDamageTypeBuilder Invert =>
            new DamageTypeBuilderStub($"Invert({this})");

        public IDamageTypeBuilder Except(IDamageTypeBuilder type) =>
            new DamageTypeBuilderStub($"({this}).Except({type})");

        public IStatBuilder Resistance =>
            new StatBuilderStub($"{this} Resistance");

        public IDamageStatBuilder Damage =>
            new DamageStatBuilderStub($"{this} Damage");

        public IConditionBuilder DamageOverTimeIsOn(IEntityBuilder entity) =>
            new ConditionBuilderStub($"{entity} is affected by {this} Damage over Time");

        public IStatBuilder Penetration =>
            new StatBuilderStub($"{this} Penetration");

        public IFlagStatBuilder IgnoreResistance =>
            new FlagStatBuilderStub($"Ignore {this} Resistance");
    }


    public class DamageTypeBuildersStub : IDamageTypeBuilders
    {
        public IDamageTypeBuilder Physical =>
            new DamageTypeBuilderStub("Physical");

        public IDamageTypeBuilder Fire => 
            new DamageTypeBuilderStub("Fire");

        public IDamageTypeBuilder Lightning =>
            new DamageTypeBuilderStub("Lightning");

        public IDamageTypeBuilder Cold => 
            new DamageTypeBuilderStub("Cold");

        public IDamageTypeBuilder Chaos => 
            new DamageTypeBuilderStub("Chaos");

        public IDamageTypeBuilder RandomElement =>
            new DamageTypeBuilderStub("Random Element");
    }
}