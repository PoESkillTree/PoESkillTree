namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <summary>
    /// Factory interface for effects.
    /// </summary>
    public interface IEffectBuilders
    {
        /// <summary>
        /// Gets an effect representing stuns.
        /// </summary>
        IStunEffectBuilder Stun { get; }

        /// <summary>
        /// Gets an effect representing knockbacks.
        /// </summary>
        IKnockbackEffectBuilder Knockback { get; }

        /// <summary>
        /// Gets a factory for ailment effects.
        /// </summary>
        IAilmentBuilders Ailment { get; }

        /// <summary>
        /// Gets a factory for ground effects.
        /// </summary>
        IGroundEffectBuilders Ground { get; }
    }
}