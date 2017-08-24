using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IProjectileStatBuilders
    {
        IStatBuilder Speed { get; }

        IStatBuilder Count { get; }

        IStatBuilder PierceCount { get; }
        IConditionBuilder Pierces { get; }

        IStatBuilder ChainCount { get; }

        // value is user entered, default is 35
        IStatBuilder TravelDistance { get; }
    }
}