namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Factory interface for dodge stats.
    /// </summary>
    public interface IDodgeStatBuilders
    {
        /// <summary>
        /// Gets a stat representing the chance to dodge attacks.
        /// </summary>
        IStatBuilder AttackChance { get; }

        /// <summary>
        /// Gets a stat representing the chance to dodge spells.
        /// </summary>
        IStatBuilder SpellChance { get; }
    }
}