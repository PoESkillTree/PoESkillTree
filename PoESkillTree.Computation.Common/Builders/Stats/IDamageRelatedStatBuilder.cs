using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents a damage related stat, e.g. damage, accuracy or crit modifiers. It can be limited by damage source,
    /// skills vs. ailments and hand (main or off-hand), and supports conditions based on weapon tags.
    /// <para>Not all stats can be limited by all of these, e.g. accuracy is only relevant for attacks.</para>
    /// </summary>
    public interface IDamageRelatedStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Limits the damage by source.
        /// </summary>
        IDamageRelatedStatBuilder With(IDamageSourceBuilder source);

        /// <summary>
        /// Limits the damage to not apply to damage over time.
        /// </summary>
        IDamageRelatedStatBuilder WithHits { get; }

        /// <summary>
        /// Limits the damage to not apply to non-ailment damage over time.
        /// </summary>
        IDamageRelatedStatBuilder WithHitsAndAilments { get; }

        /// <summary>
        /// Limits the damage to only apply to ailments.
        /// </summary>
        IDamageRelatedStatBuilder WithAilments { get; }

        /// <summary>
        /// Limits the damage to only apply to the given ailment.
        /// </summary>
        IDamageRelatedStatBuilder With(IAilmentBuilder ailment);

        /// <summary>
        /// Limits the damage to only apply to attacks with the given hand.
        /// </summary>
        IDamageRelatedStatBuilder With(AttackDamageHand hand);
    }
}