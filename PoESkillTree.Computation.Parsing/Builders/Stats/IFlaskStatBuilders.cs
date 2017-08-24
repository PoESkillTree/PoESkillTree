using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IFlaskStatBuilders
    {
        IStatBuilder Effect { get; }
        IStatBuilder Duration { get; }

        IStatBuilder LifeRecovery { get; }
        IStatBuilder ManaRecovery { get; }
        IStatBuilder RecoverySpeed { get; }

        IStatBuilder ChargesUsed { get; }
        IStatBuilder ChargesGained { get; }

        IConditionBuilder IsAnyActive { get;  }
    }
}