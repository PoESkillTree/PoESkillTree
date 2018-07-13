using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Skills
{
    /// <summary>
    /// Represents a collection of skills.
    /// </summary>
    /// <remarks>
    /// The stat properties that are the same as in <see cref="ISkillBuilder"/> only make sense as modifiers applied
    /// to the stats of skills in the collection.
    /// </remarks>
    public interface ISkillBuilderCollection : IBuilderCollection<ISkillBuilder>
    {
        /// <summary>
        /// Gets an action that occurs when Self casts any skill in this collection.
        /// </summary>
        IActionBuilder Cast { get; }

        /// <summary>
        /// Gets a stat representing the number of active instances of all skills in this collection combined 
        /// (cast by Self).
        /// </summary>
        IStatBuilder CombinedInstances { get; }
    }
}