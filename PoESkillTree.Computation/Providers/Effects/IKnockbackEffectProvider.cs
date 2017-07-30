using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Effects
{
    public interface IKnockbackEffectProvider : IEffectProvider
    {
        IStatProvider Distance { get; }
    }
}