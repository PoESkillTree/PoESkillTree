namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IFlagStatProviderFactory
    {
        IFlagStatProvider Onslaught { get; }

        IFlagStatProvider UnholyMight { get; }

        IFlagStatProvider Phasing { get; }

        IFlagStatProvider IgnoreMovementSpeedPenalties { get; }
    }
}