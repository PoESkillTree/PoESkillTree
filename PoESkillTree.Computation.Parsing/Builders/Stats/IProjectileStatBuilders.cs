using PoESkillTree.Computation.Parsing.Builders.Actions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IProjectileStatBuilders
    {
        IStatBuilder Speed { get; }

        IStatBuilder Count { get; }

        IStatBuilder PierceCount { get; }
        ISelfToAnyActionBuilder Pierce { get; }

        IStatBuilder ChainCount { get; }

        // value is user entered, default is 35
        IStatBuilder TravelDistance { get; }
    }
}