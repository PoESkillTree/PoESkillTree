namespace PoESkillTree.Computation.Providers
{
    public interface IChargeTypeProvider
    {
        IStatProvider Amount { get; }
        IStatProvider Duration { get; }
        IStatProvider ChanceToGain { get; }
    }


    public interface IChargeTypeProviderFactory
    {
        IChargeTypeProvider Endurance { get; }
        IChargeTypeProvider Frenzy { get; }
        IChargeTypeProvider Power { get; }
    }


    public static class ChargeTypeProviders
    {
        public static readonly IChargeTypeProviderFactory Charge;
    }
}