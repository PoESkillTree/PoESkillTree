namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Factory interface for attribute stats.
    /// </summary>
    public interface IAttributeStatBuilders
    {
        IStatBuilder Strength { get; }
        IStatBuilder Dexterity { get; }
        IStatBuilder Intelligence { get; }

        /// <summary>
        /// Gets a stat representing the bonus to (by default) melee damage gained from (by default) strength.
        /// </summary>
        IStatBuilder StrengthDamageBonus { get; }

        /// <summary>
        /// Gets a stat representing the increase of (by default) evasion gained from (by default) evasion.
        /// </summary>
        IStatBuilder DexterityEvasionBonus { get; }
    }
}