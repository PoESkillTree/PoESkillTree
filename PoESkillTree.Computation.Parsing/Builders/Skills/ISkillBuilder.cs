using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    public interface ISkillBuilder
    {
        IActionBuilder<ISelfBuilder, IEntityBuilder> Cast { get; }

        IStatBuilder Instances { get; }
        // shortcut for Instances.Value > 0
        IConditionBuilder HasInstance { get; }

        IStatBuilder Duration { get; }

        IStatBuilder Cost { get; }
        IStatBuilder Reservation { get; }

        IStatBuilder CooldownRecoverySpeed { get; }

        IStatBuilder DamageEffectiveness { get; } // default value: 100

        // attack/cast rate (casts per second)
        IStatBuilder Speed { get; }

        IStatBuilder AreaOfEffect { get; }
    }
}