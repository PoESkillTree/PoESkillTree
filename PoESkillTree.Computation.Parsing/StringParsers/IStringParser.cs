namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <summary>
    /// Generic interface for parsing modifier lines.
    /// </summary>
    /// <typeparam name="TResult">The type of parsing results.</typeparam>
    public interface IStringParser<TResult>
    {
        /// <summary>
        /// Parses the given modifier line into <see cref="StringParseResult{T}"/>.
        /// </summary>
        /// <param name="modifierLine">the modifier line that should be parsed</param>
        /// <remarks>
        /// Throws <see cref="Common.Parsing.ParseException"/> if the data specification is erroneous, e.g. it tries to
        /// reference values that don't occur in the matched modifier. You'll want to handle modifiers that throw a
        /// <see cref="Common.Parsing.ParseException"/> on being parsed like not parseable modifiers.
        /// </remarks>
        StringParseResult<TResult> Parse(string modifierLine);
    }
}