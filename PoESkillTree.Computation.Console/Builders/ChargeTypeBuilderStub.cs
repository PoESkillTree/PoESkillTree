using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ChargeTypeBuilderStub : BuilderStub, IChargeTypeBuilder
    {
        private readonly IConditionBuilders _conditionBuilders;

        public ChargeTypeBuilderStub(string stringRepresentation, 
            IConditionBuilders conditionBuilders) : base(stringRepresentation)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IStatBuilder Amount => new StatBuilderStub(this + " amount", _conditionBuilders);

        public IStatBuilder Duration => new StatBuilderStub(this + " duration", _conditionBuilders);

        public IStatBuilder ChanceToGain =>
            new StatBuilderStub(this + " chance to gain", _conditionBuilders);
    }


    public class ChargeTypeBuildersStub : IChargeTypeBuilders
    {
        private readonly IConditionBuilders _conditionBuilders;

        public ChargeTypeBuildersStub(IConditionBuilders conditionBuilders)
        {
            _conditionBuilders = conditionBuilders;
        }

        public IChargeTypeBuilder Endurance =>
            new ChargeTypeBuilderStub("Endurance Charge", _conditionBuilders);

        public IChargeTypeBuilder Frenzy =>
            new ChargeTypeBuilderStub("Frenzy Charge", _conditionBuilders);

        public IChargeTypeBuilder Power =>
            new ChargeTypeBuilderStub("Power Charge", _conditionBuilders);
    }
}