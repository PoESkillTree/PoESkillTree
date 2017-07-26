namespace PoESkillTree.Computation.Providers
{
    public interface IChargeTypeProvider
    {
        IStatProvider Amount { get; }
        IStatProvider Duration { get; }
        IStatProvider ChanceToGain { get; }
    }


    public static class ChargeTypeProviders
    {
        public static readonly IChargeTypeProvider EnduranceCharge;
        public static readonly IChargeTypeProvider FrenzyCharge;
        public static readonly IChargeTypeProvider PowerCharge;
    }
}