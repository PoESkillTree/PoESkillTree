using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
{
    /// <summary>
    /// Represents an effect entities can be affected by, e.g. buffs, ailments or stun.
    /// </summary>
    public interface IEffectBuilder : IResolvable<IEffectBuilder>
    {
        /// <summary>
        /// Returns a flag stat representing whether <paramref name="target"/> is currently affected by this effect.
        /// </summary>
        IFlagStatBuilder On(IEntityBuilder target);

        /// <summary>
        /// Returns a stat representing the chance to inflict this effect upon entities of type 
        /// <paramref name="target"/>. The type of action the chance applies to must be specified with a condition.
        /// </summary>
        IStatBuilder ChanceOn(IEntityBuilder target);

        /// <summary>
        /// Returns a condition that is satisfied if <paramref name="target"/> is currently affected by this effect.
        /// </summary>
        /// <remarks>
        /// Equivalent to <c>On(target).IsSet</c>
        /// </remarks>
        IConditionBuilder IsOn(IEntityBuilder target);

        /// <summary>
        /// Gets a stat representing the duration of this effect when inflicted by Self (not necessarily upon Self).
        /// </summary>
        IStatBuilder Duration { get; }
    }
}