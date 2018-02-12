using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Effects
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the knockback effect.
    /// </summary>
    public interface IKnockbackEffectBuilder : IEffectBuilder
    {
        /// <summary>
        /// Gets a stat representing the distance of knockbacks inflicted by Self.
        /// </summary>
        IStatBuilder Distance { get; }
    }
}