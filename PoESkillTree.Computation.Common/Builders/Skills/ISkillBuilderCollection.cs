using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Entities;
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
        /// Gets a new collection including all skills in this collection that have all the given keywords.
        /// </summary>
        ISkillBuilderCollection this[params IKeywordBuilder[] keywords] { get; }

        /// <summary>
        /// Gets an action that occurs when Self casts any skill in this collection.
        /// </summary>
        IActionBuilder Cast { get; }

        /// <summary>
        /// Gets a stat representing the number of active instances of all skills in this collection combined 
        /// (cast by Self).
        /// </summary>
        IStatBuilder CombinedInstances { get; }

        /// <summary>
        /// Returns a flag stat representing whether stats granted by skills in this collection additionally apply to 
        /// <paramref name="entity"/>.
        /// <para>I.e. stats granted by skills in this collection that do not apply to <paramref name="entity"/> are 
        /// copied to also apply to <paramref name="entity"/>.</para>
        /// </summary>
        /// <remarks>
        /// See "Your Offering Skills also affect you".
        /// <para>This is similar to <see cref="Conditions.IConditionBuilders.For"/> but for the stats granted by skills in this 
        /// collection instead of the currently parsed stat itself.</para>
        /// </remarks>
        IFlagStatBuilder ApplyStatsToEntity(IEntityBuilder entity);
    }
}