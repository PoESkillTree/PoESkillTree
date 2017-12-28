namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats related to gems.
    /// </summary>
    public interface IGemStatBuilders
    {
        /// <summary>
        /// Returns a stat representing the modifier to the level of gems.
        /// </summary>
        /// <param name="onlySupportGems">True if the stat should only modify support gems.</param>
        /// <remarks>
        /// This stat is used locally to increase the level of socketed gems.
        /// </remarks>
        IStatBuilder IncreaseLevel(bool onlySupportGems = false);
    }
}