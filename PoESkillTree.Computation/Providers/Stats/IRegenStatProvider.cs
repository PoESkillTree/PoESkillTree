namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IRegenStatProvider : IStatProvider
    {
        // Percent will be added to stat behind IRegenProvider
        IStatProvider Percent { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Regen property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatProvider AppliesTo(IPoolStatProvider stat);
    }
}