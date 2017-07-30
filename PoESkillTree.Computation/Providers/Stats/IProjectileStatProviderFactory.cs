using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IProjectileStatProviderFactory
    {
        IStatProvider Speed { get; }

        IStatProvider Count { get; }

        IStatProvider PierceCount { get; }
        IConditionProvider Pierces { get; }

        IStatProvider ChainCount { get; }

        // value is user entered, default is 35
        IStatProvider TravelDistance { get; }
    }
}