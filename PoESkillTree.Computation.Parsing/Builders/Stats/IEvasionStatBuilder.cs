namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Represents the evasion rating stat.
    /// </summary>
    public interface IEvasionStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Returns a stat representing the chance to evade unspecific attacks.
        /// </summary>
        IStatBuilder Chance { get; }

        /// <summary>
        /// Returns a stat representing the chance to evade projectile attacks.
        /// </summary>
        IStatBuilder ChanceAgainstProjectileAttacks { get; }

        /// <summary>
        /// Returns a stat representing the chance to evade melee attacks.
        /// </summary>
        IStatBuilder ChanceAgainstMeleeAttacks { get; }
    }
}