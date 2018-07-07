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
        IDamageRelatedStatBuilder With(DamageSource source);

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
        /// Limits the damage to not apply to ailments.
        /// </summary>
        IDamageRelatedStatBuilder WithSkills { get; }

        /// <summary>
        /// Limits the damage to only apply to attacks with the given hand.
        /// </summary>
        IDamageRelatedStatBuilder With(AttackDamageHand hand);

        /// <summary>
        /// Returns a stat that specifies whether modifiers to this stat under the current limitations also apply to
        /// the given source (but not ailments) if they have any of the given forms.
        /// <para>The stat's value specifies a multiplier. It should be 100% in most cases.</para>
        /// <para>If no form is given, it applies to all forms.</para>
        /// </summary>
        IStatBuilder ApplyModifiersToSkills(DamageSource source, params Form[] forms);

        /// <summary>
        /// Returns a stat that specifies whether modifiers to this stat under the current limitations also apply to
        /// ailments if they have any of the given forms.
        /// <para>The stat's value specifies a multiplier. It should be 100% in most cases.</para>
        /// <para>If no form is given, it applies to all forms.</para>
        /// </summary>
        IStatBuilder ApplyModifiersToAilments(params Form[] forms);
    }
}