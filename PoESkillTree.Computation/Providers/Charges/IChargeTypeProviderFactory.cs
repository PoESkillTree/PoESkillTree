namespace PoESkillTree.Computation.Providers.Charges
{
    public interface IChargeTypeProviderFactory
    {
        IChargeTypeProvider Endurance { get; }
        IChargeTypeProvider Frenzy { get; }
        IChargeTypeProvider Power { get; }
    }
}