namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IRegenStatBuilder : IStatBuilder
    {
        // Percent will be added to stat behind IRegenProvider
        IStatBuilder Percent { get; }

        // Set to 1 with Form.BaseSet for the pool stat from whose Regen property this instance originated.
        // If 1 (with Form.TotalOverride) for any other pool stat, that one applies.
        IFlagStatBuilder AppliesTo(IPoolStatBuilder stat);
    }
}