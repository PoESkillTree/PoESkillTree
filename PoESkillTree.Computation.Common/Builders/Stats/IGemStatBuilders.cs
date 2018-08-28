namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats related to gems.
    /// </summary>
    public interface IGemStatBuilders
    {
        /// <summary>
        /// A stat representing the modifier to the level of support gems.
        /// </summary>
        /// <remarks>
        /// This stat is used locally to increase the level of socketed gems.
        /// </remarks>
        IStatBuilder IncreaseSupportLevel { get; }
    }
}