using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class DamageTypeBuilderStub : BuilderStub, IDamageTypeBuilder
    {
        private readonly IConditionBuilders _conditionBuilders;

        public DamageTypeBuilderStub(string stringRepresentation,
            IConditionBuilders conditionBuilders) : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IDamageTypeBuilder And(IDamageTypeBuilder type) =>
            new DamageTypeBuilderStub($"{this}, {type}", _conditionBuilders);

        public IDamageTypeBuilder Invert =>
            new DamageTypeBuilderStub($"Invert({this})", _conditionBuilders);

        public IDamageTypeBuilder Except(IDamageTypeBuilder type) =>
            new DamageTypeBuilderStub($"({this}).Except({type})", _conditionBuilders);

        public IStatBuilder Resistance =>
            new StatBuilderStub($"{this} Resistance", _conditionBuilders);

        public IDamageStatBuilder Damage =>
            new DamageStatBuilderStub($"{this} Damage", _conditionBuilders);

        public IConditionBuilder DamageOverTimeIsOn(IEntityBuilder entity) =>
            new ConditionBuilderStub($"{entity} is affected by {this} Damage over Time");

        public IStatBuilder Penetration =>
            new StatBuilderStub($"{this} Penetration", _conditionBuilders);

        public IFlagStatBuilder IgnoreResistance =>
            new FlagStatBuilderStub($"Ignore {this} Resistance", _conditionBuilders);
    }


    public class DamageTypeBuildersStub : IDamageTypeBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public DamageTypeBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IDamageTypeBuilder Physical =>
            new DamageTypeBuilderStub("Physical", _conditionBuilders);

        public IDamageTypeBuilder Fire => 
            new DamageTypeBuilderStub("Fire", _conditionBuilders);

        public IDamageTypeBuilder Lightning =>
            new DamageTypeBuilderStub("Lightning", _conditionBuilders);

        public IDamageTypeBuilder Cold => 
            new DamageTypeBuilderStub("Cold", _conditionBuilders);

        public IDamageTypeBuilder Chaos => 
            new DamageTypeBuilderStub("Chaos", _conditionBuilders);

        public IDamageTypeBuilder RandomElement =>
            new DamageTypeBuilderStub("Random Element", _conditionBuilders);
    }
}