using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.Builders.Actions
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an action that occurs when Self blocks a hit from any entity and contains stats related to blocking.
    /// </summary>
    public interface IBlockActionBuilder : IActionBuilder
    {
        /// <summary>
        /// Gets a stat representing block recovery.
        /// </summary>
        IStatBuilder Recovery { get; }

        /// <summary>
        /// Gets a stat representing block chance against attacks.
        /// </summary>
        IStatBuilder AttackChance { get; }

        /// <summary>
        /// Gets a stat representing block chance against spells.
        /// </summary>
        IStatBuilder SpellChance { get; }
    }
}