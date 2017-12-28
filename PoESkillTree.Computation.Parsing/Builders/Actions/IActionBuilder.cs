using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Damage;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    /// <summary>
    /// Represents an action taken by an <see cref="IEntityBuilder"/> against an <see cref="IEntityBuilder"/>.
    /// </summary>
    public interface IActionBuilder : IResolvable<IActionBuilder>
    {
        /// <summary>
        /// The entity executing the action.
        /// </summary>
        IEntityBuilder Source { get; }

        /// <summary>
        /// The target entity of the action.
        /// </summary>
        IEntityBuilder Target { get; }

        /// <summary>
        /// Returns an action identical to this action but executed by <paramref name="source"/>.
        /// </summary>
        IActionBuilder By(IEntityBuilder source);

        /// <summary>
        /// Returns an action identical to this action but targeted against <paramref name="target"/>.
        /// </summary>
        IActionBuilder Against(IEntityBuilder target);

        /// <summary>
        /// Gets an action identical to this action but executed by <see cref="Target"/> and targeted against
        /// <see cref="Source"/>.
        /// </summary>
        IActionBuilder Taken { get; }

        /// <summary>
        /// Returns an action identical to this action but with the additional condition that the action must occur
        /// by damage of type <paramref name="damageType"/> (by damage of any type in <paramref name="damageType"/> if
        /// it is a collection).
        /// </summary>
        IActionBuilder With(IDamageTypeBuilder damageType);

        /// <summary>
        /// Returns a condition that is satisfied when this action is executed.
        /// </summary>
        IConditionBuilder On();

        /// <summary>
        /// Returns a condition that is satisfied when this action is executed by a skill having the keyword
        /// <paramref name="withKeyword"/>.
        /// </summary>
        IConditionBuilder On(IKeywordBuilder withKeyword);

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