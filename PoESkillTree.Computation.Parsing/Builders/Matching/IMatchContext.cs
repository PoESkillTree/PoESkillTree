namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    /// <summary>
    /// Collection that depends on the matched stat line as context to get the final values.
    /// <para>When called in the Data project, returned instances need to be resolved to get the final values.
    /// This implies that out of bounds exceptions can not be thrown before the context is resolved.</para>
    /// </summary>
    /// <typeparam name="T">The type of the values in this collection.</typeparam>
    public interface IMatchContext<out T>
    {
        /// <summary>
        /// Gets the value at the given index.
        /// </summary>
        T this[int index] { get; }

        /// <summary>
        /// Gets the first value.
        /// </summary>
        T First { get; }

        /// <summary>
        /// Gets the last value.
        /// </summary>
        T Last { get; }

        /// <summary>
        /// Gets the only value.
        /// </summary>
        T Single { get; }
    }
}