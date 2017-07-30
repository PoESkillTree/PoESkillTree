using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Charges
{
    public interface IChargeTypeProvider
    {
        IStatProvider Amount { get; }
        IStatProvider Duration { get; }
        IStatProvider ChanceToGain { get; }
    }
}