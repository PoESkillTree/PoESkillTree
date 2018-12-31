using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <summary>
    /// Represents an effect entities can be affected by, e.g. buffs, ailments or stun.
    /// </summary>
    public interface IEffectBuilder : IResolvable<IEffectBuilder>
    {
        /// <summary>
        /// Returns a stat representing whether <paramref name="target"/> is currently affected by this effect.
        /// </summary>
        IStatBuilder On(IEntityBuilder target);

        /// <summary>
        /// Returns a stat representing the chance to inflict/activate this effect.
        /// The type of action the chance applies to must be specified with a condition.
        /// </summary>
        IDamageRelatedStatBuilder Chance { get; }

        /// <summary>
        /// Returns a condition that is satisfied if <paramref name="target"/> is currently affected by this effect.
        /// </summary>
        IConditionBuilder IsOn(IEntityBuilder target);

        /// <summary>
        /// Gets a stat representing the duration of this effect when inflicted by Self (not necessarily upon Self).
        /// </summary>
        IStatBuilder Duration { get; }

        /// <summary>
        /// Adds <paramref name="stat"/> to the stats of this effect if Self is affected by it.
        /// Modifiers with stats built from the returned stat will apply their value to <paramref name="stat"/>
        /// modified by this effect's increase.
        /// <para>E.g. "Consecrated Ground grants you 40% increased Damage"</para>
        /// </summary>
        IStatBuilder AddStat(IStatBuilder stat);

        /// <summary>
        /// Builds this effect to its identity string.
        /// </summary>
        string Build(BuildParameters parameters);
    }
}