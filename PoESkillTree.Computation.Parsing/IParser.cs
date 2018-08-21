namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Generic interface for parsers that use <see cref="ParseResult"/> as result type.
    /// </summary>
    /// <typeparam name="TParameter">Parsing parameter type</typeparam>
    public interface IParser<in TParameter>
    {
        /// <summary>
        /// Parses the given parameter into <see cref="ParseResult"/>.
        /// </summary>
        ParseResult Parse(TParameter parameter);
    }
}