using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Entities
{
    /// <summary>
    /// Represents enemy entities.
    /// </summary>
    public interface IEnemyBuilder : IEntityBuilder
    {
        /// <summary>
        /// Gets a condition that is satisfied if this enemy is near Self.
        /// </summary>
        IConditionBuilder IsNearby { get; }

        /// <summary>
        /// Gets a condition that is satisfied if this enemy is Rare. 
        /// </summary>
        IConditionBuilder IsRare { get; }

        /// <summary>
        /// Gets a condition that is satisfied if this enemy is Unique. 
        /// </summary>
        IConditionBuilder IsUnique { get; }

        /// <summary>
        /// Gets a condition that is satisfied if this enemy is Rare or Unique.
        /// </summary>
        IConditionBuilder IsRareOrUnique { get; }
    }
}