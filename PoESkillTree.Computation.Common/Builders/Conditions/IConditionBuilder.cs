using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Common.Builders.Conditions
{
    /// <summary>
    /// Represents a condition.
    /// </summary>
    public interface IConditionBuilder : IResolvable<IConditionBuilder>
    {
        /// <summary>
        /// Returns a new condition that is satisfied if this condition and <paramref name="condition"/> are satisfied.
        /// </summary>
        IConditionBuilder And(IConditionBuilder condition);

        /// <summary>
        /// Returns a new condition that is satisfied if this condition or <paramref name="condition"/> is satisfied.
        /// </summary>
        IConditionBuilder Or(IConditionBuilder condition);

        /// <summary>
        /// Returns a new condition that is satisfied if this condition is not satisfied.
        /// </summary>
        IConditionBuilder Not { get; }
    }
}