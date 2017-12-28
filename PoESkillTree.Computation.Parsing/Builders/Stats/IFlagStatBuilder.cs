using PoESkillTree.Computation.Parsing.Builders.Conditions;

namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Represents a stat that only allows 0 (deactivated/off) and 1 (activated/on) as values.
    /// <para>A flag stat being activated usually has other consequences, e.g. non-buff stats like Onslaught being 
    /// applied or effects (like buffs and auras) being applied.</para>
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

        /// <summary>
        /// Gets a stat representing the modifier to stats being applied when this flag is on.
        /// </summary>
        IStatBuilder Effect { get; }

        /// <summary>
        /// Gets a stat representing the duration for which this flag stays on.
        /// </summary>
        IStatBuilder Duration { get; }
    }
}