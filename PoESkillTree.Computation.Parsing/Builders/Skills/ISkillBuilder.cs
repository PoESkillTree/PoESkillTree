using PoESkillTree.Computation.Parsing.Builders.Actions;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    /// <summary>
    /// Represents a skill.
    /// </summary>
    public interface ISkillBuilder : IResolvable<ISkillBuilder>
    {
        /// <summary>
        /// Gets an action that occurs when Self casts this skill.
        /// </summary>
        IActionBuilder Cast { get; }

        /// <summary>
        /// Gets a stat representing the number of active instances of this skill (cast by Self).
        /// (e.g. the number of zombies cast by Raise Zombie)
        /// </summary>
        IStatBuilder Instances { get; }

        /// <summary>
        /// Gets a condition that is satisfied if there are active instances of this skill (cast by Self).
        /// </summary>
        /// <remarks>
        /// Shortcut for <c>Instances.Value > 0</c>.
        /// </remarks>
        IConditionBuilder HasInstance { get; }

        /// <summary>
        /// Gets a stat representing the duration of this skill.
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// Gets a stat representing the mana cost of this skill.
        /// </summary>
        IStatBuilder Cost { get; }

        /// <summary>
        /// Gets a stat representing the mana reservation of this skill.
        /// </summary>
        IStatBuilder Reservation { get; }

        /// <summary>
        /// Gets a stat representing the cooldown recovery speed of this skill.
        /// </summary>
        IStatBuilder CooldownRecoverySpeed { get; }

        /// <summary>
        /// Gets a stat representing the damage effectiveness of this skill.
        /// </summary>
        IStatBuilder DamageEffectiveness { get; }

        /// <summary>
        /// Gets a stat representing attack/cast rate of this skill (in casts per second).
        /// </summary>
        IStatBuilder Speed { get; }

        /// <summary>
        /// Gets a stat representing the area of effect of this skill.
        /// </summary>
        IStatBuilder AreaOfEffect { get; }
    }
}