using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IRechargeStatBuilder : IStatBuilder
    {
        // 2 / RechargeStart.Value is the delay in seconds
        IStatBuilder Start { get; } // default value: 1

        IConditionBuilder StartedRecently { get; }
    }
}