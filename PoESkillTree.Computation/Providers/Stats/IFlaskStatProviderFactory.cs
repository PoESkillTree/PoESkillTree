using PoESkillTree.Computation.Providers.Conditions;

namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IFlaskStatProviderFactory
    {
        IStatProvider Effect { get; }
        IStatProvider Duration { get; }

        IStatProvider LifeRecovery { get; }
        IStatProvider ManaRecovery { get; }
        IStatProvider RecoverySpeed { get; }

        IStatProvider ChargesUsed { get; }
        IStatProvider ChargesGained { get; }

        IConditionProvider IsAnyActive { get;  }
    }
}