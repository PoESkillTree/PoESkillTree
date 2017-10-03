using PoESkillTree.Computation.Parsing.Builders.Charges;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ChargeTypeBuilderStub : BuilderStub, IChargeTypeBuilder
    {
        public ChargeTypeBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }

        public IStatBuilder Amount => new StatBuilderStub(this + " amount");

        public IStatBuilder Duration => new StatBuilderStub(this + " duration");

        public IStatBuilder ChanceToGain =>
            new StatBuilderStub(this + " chance to gain");
    }


    public class ChargeTypeBuildersStub : IChargeTypeBuilders
    {
        public IChargeTypeBuilder Endurance =>
            new ChargeTypeBuilderStub("Endurance Charge");

        public IChargeTypeBuilder Frenzy =>
            new ChargeTypeBuilderStub("Frenzy Charge");

        public IChargeTypeBuilder Power =>
            new ChargeTypeBuilderStub("Power Charge");
    }
}