using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Effects
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