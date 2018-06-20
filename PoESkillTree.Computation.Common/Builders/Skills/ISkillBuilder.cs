using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Skills
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
        /// This skill's identifier.
        /// </summary>
        ValueBuilder SkillId { get; }
    }
}