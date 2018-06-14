using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents a modifiable statistic of an entity.
    /// </summary>
    public interface IStatBuilder : IResolvable<IStatBuilder>
    {
        /// <summary>
        /// Gets a stat representing the minimum value this stat can be modified to.
        /// Defaults to negative infinity.
        /// </summary>
        /// <remarks>
        /// The minimum has no effect if this stat has a base value (through BaseSet, BaseAdd and BaseSubtract forms)
        /// of 0. That is necessary to make sure Unarmed and Incinerate can't crit as long they don't get base crit 
        /// chance.
        /// </remarks>
        IStatBuilder Minimum { get; }

        /// <summary>
        /// Gets a stat representing the maximum value this stat can be modified to.
        /// Defaults to positive infinity.
        /// </summary>
        IStatBuilder Maximum { get; }

        /// <summary>
        /// Gets this stat's value. Defaults to 0.
        /// </summary>
        ValueBuilder Value { get; }

        /// <summary>
        /// Returns a stat that represents the percentage of this stat's value that is converted to the given stat.
        /// </summary>
        IStatBuilder ConvertTo(IStatBuilder stat);

        /// <summary>
        /// Returns a stat that represents the percentage of this stat's value that is added to the given stat.
        /// </summary>
        IStatBuilder GainAs(IStatBuilder stat);

        /// <summary>
        /// Gets a stat representing the chance to double this stat's value (does not make sense without an action
        /// condition. E.g. damage has a 20% chance to be doubled on hit.
        /// </summary>
        IStatBuilder ChanceToDouble { get; }

        /// <summary>
        /// Applies this stat to <paramref name="entity"/> instead of the currently modified entity.
        /// See <see cref="IConditionBuilders.For"/> for more information.
        /// </summary>
        IStatBuilder For(IEntityBuilder entity);

        /// <summary>
        /// Returns a stat that is identical to this stat but is only modified if the given condition is satisfied.
        /// <para>Should generally not be used but is necessary for including conditions in resolved stat references.
        /// </para>
        /// </summary>
        IStatBuilder WithCondition(IConditionBuilder condition);

        /// <summary>
        /// Returns a stat that combines this and the given stat. Modifiers to the returned stat will apply to both,
        /// but only once (no multiple application if one of the stats is converted to another).
        /// </summary>
        IStatBuilder CombineWith(IStatBuilder other);

        /// <summary>
        /// Builds this instance into a list of <see cref="StatBuilderResult"/>s.
        /// </summary>
        IEnumerable<StatBuilderResult> Build(BuildParameters parameters, ModifierSource originalModifierSource);
    }
}