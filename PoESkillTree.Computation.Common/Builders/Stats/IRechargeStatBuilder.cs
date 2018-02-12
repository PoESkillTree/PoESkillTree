using PoESkillTree.Computation.Common.Builders.Conditions;

namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Represents the recharge stat of a pool. Its value is the percentage of the pool recharged per second if
    /// recharge is active.
    /// </summary>
    public interface IRechargeStatBuilder : IStatBuilder
    {
        /// <summary>
        /// Gets a stat representing the modifier to the delay before recharging starts after taking damage.
        /// </summary>
        /// <remarks>
        /// <c>2 / Start.Value</c> is the recharge delay in seconds.
        /// <para>The base value is 1.</para>
        /// </remarks>
        IStatBuilder Start { get; }
        
        // "Time since the last damage was done to any pool" would probably be a better user specified value than
        // this condition for each pool.
        /// <summary>
        /// Gets a condition that is satisfied if recharging the pool started recently.
        /// </summary>
        IConditionBuilder StartedRecently { get; }
    }
}