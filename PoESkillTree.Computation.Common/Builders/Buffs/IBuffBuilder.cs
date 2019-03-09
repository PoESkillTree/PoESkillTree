using System.Collections.Generic;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Common.Builders.Buffs
{
    /// <summary>
    /// Represents buff effects.
    /// </summary>
    public interface IBuffBuilder : IEffectBuilder
    {
        /// <summary>
        /// Returns a stat representing whether <paramref name="target"/> is currently affected by this effect.
        /// This affection does not count as a buff and generic buff effect modifiers do not apply.
        /// </summary>
        IStatBuilder NotAsBuffOn(IEntityBuilder target);
        
        /// <summary>
        /// Returns a condition that is satisfied if <paramref name="target"/> is currently affected by this buff
        /// and <paramref name="source"/> applied the buff to <paramref name="target"/>.
        /// </summary>
        IConditionBuilder IsOn(IEntityBuilder source, IEntityBuilder target);

        /// <summary>
        /// Gets a stat representing the effect modifier of this buff when created by Self.
        /// </summary>
        IStatBuilder Effect { get; }

        IStatBuilder EffectOn(IEntityBuilder target);

        IStatBuilder AddStatForSource(IStatBuilder stat, IEntityBuilder source);

        /// <summary>
        /// The number of stacks this buff currently has. Only relevant for buffs that can stack, the stat does
        /// nothing for most buffs. The value has to be entered by the user.
        /// </summary>
        IStatBuilder StackCount { get; }

        /// <summary>
        /// Builds an IValue that calculates the multiplier to the value of modifiers created from
        /// <see cref="AddStatForSource"/> and <see cref="IEffectBuilder.AddStat"/>. The ModifierSourceEntity is
        /// used as the target.
        /// </summary>
        IValue BuildAddStatMultiplier(BuildParameters parameters, IReadOnlyCollection<Entity> possibleSources);
    }
}