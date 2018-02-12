namespace PoESkillTree.Computation.Data.Steps
{
    /// <summary>
    /// Enumeration of the parsing steps, each representing an IStatMatchers instance (except <see cref="Success"/> and
    /// <see cref="Failure"/>)
    /// </summary>
    public enum ParsingStep
    {
        /// <summary>
        /// The parsing step representing a successfully completed stat parse.
        /// </summary>
        Success,
        /// <summary>
        /// The parsing step representing a failed stat parse.
        /// </summary>
        Failure,
        Special,
        StatManipulator,
        ValueConversion,
        FormAndStat,
        Form,
        GeneralStat,
        DamageStat,
        PoolStat,
        Condition
    }
}