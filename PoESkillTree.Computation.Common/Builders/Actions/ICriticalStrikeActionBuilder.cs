using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Actions
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an action that occurs when Self critically hits from any entity and contains stats related to 
    /// critical strikes.
    /// </summary>
    public interface ICriticalStrikeActionBuilder : IActionBuilder
    {
        /// <summary>
        /// Gets a stat representing the critical strike chance.
        /// </summary>
        IDamageRelatedStatBuilder Chance { get; }
        
        /// <summary>
        /// Gets a stat representing the critical strike multiplier.
        /// </summary>
        IDamageRelatedStatBuilder Multiplier { get; }
        
        /// <summary>
        /// Gets a stat representing the extra damage taken from critical strikes.
        /// </summary>
        IStatBuilder ExtraDamageTaken { get; }
    }
}