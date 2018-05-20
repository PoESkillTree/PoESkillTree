using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;
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
        /// Gets this stats value. Defaults to 0.
        /// </summary>
        ValueBuilder Value { get; }

        /// <summary>
        /// Returns a stat that represents the percentage of this stat's value that is converted to the given stat.
        /// </summary>
        IStatBuilder ConvertTo(IStatBuilder stat);

        /// <summary>
        /// Returns a stat that represents the percentage of this stat's value that is added to the given stat.
        /// </summary>
        IStatBuilder AddAs(IStatBuilder stat);

        /// <summary>
        /// Returns a stat representing whether modifiers (except of BaseSet, BaseOverride and TotalOverride forms)
        /// to this stat's value also apply to the given stat
        /// (at <paramref name="percentOfTheirValue"/> percent of their value).
        /// </summary>
        IFlagStatBuilder ApplyModifiersTo(IStatBuilder stat, IValueBuilder percentOfTheirValue);

        /// <summary>
        /// Gets a stat representing the chance to double this stat's value (does not make sense without an action
        /// condition. E.g. damage has a 20% chance to be doubled on hit.
        /// </summary>
        IStatBuilder ChanceToDouble { get; }

        // For Buffs and Auras: some conditions apply to this stat (e.g. the "Attack" from "Attack Speed"), others 
        // specify whether the buff/aura is granted (e.g. "if you've Blocked Recently"). It has to be decided at some 
        // point which conditions do and which don't. That point must be before the conditions are combined into one.
        // Probably as a property inherent in conditions, i.e. decided on condition construction.

        /// <summary>
        /// Returns a buff that modifies this stat for <paramref name="seconds"/> seconds.
        /// </summary>
        IBuffBuilder ForXSeconds(IValueBuilder seconds);

        /// <summary>
        /// Gets a buff that modifies this stat. If the buff is not permanent, the duration will be specified elsewhere,
        /// e.g. as part of a buff rotation.
        /// </summary>
        IBuffBuilder AsBuff { get; }

        /// <summary>
        /// Returns an aura affecting the given entities that modifies these stats.
        /// </summary>
        IFlagStatBuilder AsAura(params IEntityBuilder[] affectedEntities);

        /// <summary>
        /// Returns a flag stat representing whether the given skills modifies this stat as part of their effects
        /// (unaffected by effect increases).
        /// <para>E.g. "Auras you Cast grant 3% increased Attack and Cast Speed to you and Allies"</para>
        /// </summary>
        IFlagStatBuilder AddTo(ISkillBuilderCollection skills);

        /// <summary>
        /// Returns a flag stat representing whether the given effect modifies this stat as part of its effects
        /// (unaffected by effect increases).
        /// <para>E.g. "Consecrated Ground you create grants 40% increased Damage to you and Allies"</para>
        /// </summary>
        IFlagStatBuilder AddTo(IEffectBuilder effect);

        /// <summary>
        /// Returns a stat that is identical to this stat but is only modified if the given condition is satisfied.
        /// <para>Should generally not be used but is necessary for including conditions in resolved stat references.
        /// </para>
        /// </summary>
        IStatBuilder WithCondition(IConditionBuilder condition);

        /// <summary>
        /// Builds this instance into a list of <see cref="IStat"/>s, an <see cref="ModifierSource"/> converter to
        /// change the original modifier's source and a <see cref="ValueConverter"/> that should be applied
        /// to <see cref="IValueBuilder"/>s before building them.
        /// </summary>
        (IReadOnlyList<IStat> stats, Func<ModifierSource, ModifierSource> sourceConverter,
            ValueConverter valueConverter) Build();
    }
}