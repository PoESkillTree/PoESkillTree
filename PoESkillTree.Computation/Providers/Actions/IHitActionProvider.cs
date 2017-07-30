using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Actions
{
    public interface IHitActionProvider : ISelfToAnyActionProvider
    {
        IStatProvider Chance { get; }
    }
}