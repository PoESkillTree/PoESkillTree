using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents a stat that only allows 0 (deactivated/off) and 1 (activated/on) as values.
    /// <para>Flag stats are usually used to conditionally apply modifiers.</para>
    /// </summary>
    public interface IFlagStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Gets a condition that is satisfied if this stat's value is 1.
        /// </summary>
        /// <remarks>
        /// Shortcut for <c>Value == 1</c>.
        /// </remarks>
        IConditionBuilder IsSet { get; }
    }
}