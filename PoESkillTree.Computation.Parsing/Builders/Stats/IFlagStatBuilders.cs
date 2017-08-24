namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IFlagStatBuilders
    {
        IFlagStatBuilder Onslaught { get; }

        IFlagStatBuilder UnholyMight { get; }

        IFlagStatBuilder Phasing { get; }

        IFlagStatBuilder IgnoreMovementSpeedPenalties { get; }
    }
}