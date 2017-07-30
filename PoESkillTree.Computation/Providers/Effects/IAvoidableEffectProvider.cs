using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IAvoidableEffectProvider : IEffectProvider
    {
        IStatProvider Avoidance { get; }
    }
}