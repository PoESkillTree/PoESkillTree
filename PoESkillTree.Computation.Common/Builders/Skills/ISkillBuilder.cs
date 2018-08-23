using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.Skills;

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
        /// The percentage of a pool this skill reserves.
        /// </summary>
        IStatBuilder Reservation { get; }

        /// <summary>
        /// The pool this skill's reservation uses.
        /// </summary>
        IStatBuilder ReservationPool { get; }

        /// <summary>
        /// This skill's identifier.
        /// </summary>
        ValueBuilder SkillId { get; }

        /// <summary>
        /// The buff provided by this skill. Throws if this is skill does not provide a buff.
        /// </summary>
        IBuffBuilder Buff { get; }

        SkillDefinition Build();
    }
}