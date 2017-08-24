using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Charges
{
    public interface IChargeTypeBuilder
    {
        IStatBuilder Amount { get; }
        IStatBuilder Duration { get; }
        IStatBuilder ChanceToGain { get; }
    }
}