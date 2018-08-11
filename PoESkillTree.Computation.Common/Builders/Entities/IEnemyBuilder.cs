using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Common.Builders.Entities
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

        ValueBuilder CountNearby { get; }

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

        IConditionBuilder IsMoving { get; }
    }
}