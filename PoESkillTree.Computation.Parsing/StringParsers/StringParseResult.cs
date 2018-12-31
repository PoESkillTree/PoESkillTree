namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <summary>
    /// Data object for the return value of <see cref="IStringParser{TResult}.Parse"/>. Supports deconstruction to
    /// tuples and implicit conversion from tuples, allowing it to generally behave like a tuple, just with a proper
    /// type name.
    /// </summary>
    public class StringParseResult<T>
    {
        public StringParseResult(bool successfullyParsed, string remainingSubstring, T result)
            => (SuccessfullyParsed, RemainingSubstring, Result) = (successfullyParsed, remainingSubstring, result);

        public void Deconstruct(out bool successfullyParsed, out string remainingSubstring, out T result)
            => (successfullyParsed, remainingSubstring, result) = (SuccessfullyParsed, RemainingSubstring, Result);

        public static implicit operator StringParseResult<T>(
            (bool successfullyParsed, string remainingSubstring, T result) t)
            => new StringParseResult<T>(t.successfullyParsed, t.remainingSubstring, t.result);

        /// <summary>
        /// True if the modifier line was parsed successfully.
        /// </summary>
        public bool SuccessfullyParsed { get; }

        /// <summary>
        /// The substring (or concatenation of multiple substrings) of the modifier line that could not be parsed at
        /// all. Empty if <see cref="SuccessfullyParsed"/> is true.
        /// </summary>
        public string RemainingSubstring { get; }

        /// <summary>
        /// The result of parsing. Not defined if <see cref="SuccessfullyParsed"/> is false. In that case it may be
        /// null or a partial result containing null properties and should only be used for debugging purposes.
        /// </summary>
        public T Result { get; }
    }
}