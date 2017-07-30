using PoESkillTree.Computation.Providers.Actions;
using PoESkillTree.Computation.Providers.Conditions;
using PoESkillTree.Computation.Providers.Entities;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Providers.Skills
{
    public interface ISkillProvider
    {
        IActionProvider<ISelfProvider, IEntityProvider> Cast { get; }

        IStatProvider Instances { get; }
        // shortcut for Instances.Value > 0
        IConditionProvider HasInstance { get; }

        IStatProvider Duration { get; }

        IStatProvider Cost { get; }
        IStatProvider Reservation { get; }

        IStatProvider CooldownRecoverySpeed { get; }

        IStatProvider DamageEffectiveness { get; } // default value: 100

        // attack/cast rate (casts per second)
        IStatProvider Speed { get; }

        IStatProvider AreaOfEffect { get; }
    }
}