namespace PoESkillTree.Computation.Parsing.Builders.Charges
{
    public interface IChargeTypeBuilders
    {
        IChargeTypeBuilder Endurance { get; }
        IChargeTypeBuilder Frenzy { get; }
        IChargeTypeBuilder Power { get; }
    }
}