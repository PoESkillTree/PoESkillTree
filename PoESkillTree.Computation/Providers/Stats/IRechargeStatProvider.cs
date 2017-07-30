using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IRechargeStatProvider : IStatProvider
    {
        // 2 / RechargeStart.Value is the delay in seconds
        IStatProvider Start { get; } // default value: 1

        IConditionProvider StartedRecently { get; }
    }
}