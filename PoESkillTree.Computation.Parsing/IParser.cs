using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Generic interface for parsing stat lines.
    /// </summary>
    /// <typeparam name="TResult">The type of parsing results.</typeparam>
    public interface IParser<TResult>
    {
        /// <summary>
        /// If <paramref name="stat"/> was parsed successfully, the return value's 
        /// <see cref="ParseResult{T}.SuccessfullyParsed"/> is true and <see cref="ParseResult{T}.Result"/>
        /// contains the parsing result.
        /// <para>If <paramref name="stat"/> could not be parsed, <see cref="ParseResult{T}.SuccessfullyParsed"/> is
        /// false and <see cref="ParseResult{T}.Result"/> is undefined. It may be null or a partial result containing
        /// null properties and should only be used for debugging purposes.</para>
        /// <para><see cref="ParseResult{T}.RemainingStat"/> contains the parts of <paramref name="stat"/> that were
        /// not parsed into <see cref="ParseResult{T}.Result"/>.</para>
        /// </summary>
        /// <param name="stat">the stat line that should be parsed</param>
        /// <remarks>
        /// Throws <see cref="ParseException"/> if the data specification is erroneous, e.g. it tries to reference
        /// values that don't occur in the matched stat. You'll want to handle stats that throw a
        /// <see cref="ParseException"/> on being parsed like unparsable stats.
        /// </remarks>
        ParseResult<TResult> Parse(string stat);
    }

    /// <summary>
    /// This is the main interface for using Computation.Parsing. It parses stat lines to <see cref="Modifier"/>s.
    /// </summary>
    public interface IParser : IParser<IReadOnlyList<Modifier>>
    {
        /// <summary>
        /// If <paramref name="stat"/> was parsed successfully and completely, the return value's 
        /// <see cref="ParseResult.SuccessfullyParsed"/> is true, <see cref="ParseResult.Result"/>
        /// contains the parsing result and <see cref="ParseResult.RemainingStat"/> is empty.
        /// <para>If <paramref name="stat"/> could not be parsed, <see cref="ParseResult.SuccessfullyParsed"/> is
        /// false and <see cref="ParseResult.Result"/> is undefined. It may be null or contain partial
        /// <see cref="Modifier"/>s with null properties and should only be used for debugging purposes.</para>
        /// <para><see cref="ParseResult.RemainingStat"/> contains the parts of <paramref name="stat"/> that were
        /// not parsed into <see cref="ParseResult.Result"/>.</para>
        /// </summary>
        /// <param name="stat">the stat line that should be parsed</param>
        /// <remarks>
        /// Throws <see cref="ParseException"/> if the data specification is erroneous, e.g. it tries to reference
        /// values that don't occur in the matched stat. You'll want to handle stats that throw a
        /// <see cref="ParseException"/> on being parsed like unparsable stats.
        /// </remarks>
        new ParseResult Parse(string stat);
    }
}