using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Actions
{
    /// <summary>
    /// Represents an action taken by an <see cref="IEntityBuilder"/> against an <see cref="IEntityBuilder"/>.
    /// </summary>
    public interface IActionBuilder : IResolvable<IActionBuilder>
    {
        /// <summary>
        /// Returns an action identical to this action but executed by <paramref name="source"/>.
        /// </summary>
        IActionBuilder By(IEntityBuilder source);

        /// <summary>
        /// Returns a condition that is satisfied when this action is executed. Needs to be applied after all other
        /// conditions.
        /// </summary>
        IConditionBuilder On { get; }

        // seconds for all actions need to be specified by the user
        /// <summary>
        /// Returns a condition that is satisfied if this action was executed in the past <paramref name="seconds"/>
        /// seconds.
        /// </summary>
        IConditionBuilder InPastXSeconds(IValueBuilder seconds);

        /// <summary>
        /// Gets a condition that is satisfied if this action was executed in the past 4 seconds.
        /// </summary>
        IConditionBuilder Recently { get; }

        /// <summary>
        /// Returns a value indicating how often this action was executed in the past 4 seconds
        /// </summary>
        ValueBuilder CountRecently { get; }
    }
}