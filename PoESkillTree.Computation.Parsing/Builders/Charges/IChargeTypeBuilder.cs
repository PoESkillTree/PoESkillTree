using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Charges
{
    public interface IChargeTypeBuilder : IResolvable<IChargeTypeBuilder>
    {
        IStatBuilder Amount { get; }
        IStatBuilder Duration { get; }
        IStatBuilder ChanceToGain { get; }
    }
}