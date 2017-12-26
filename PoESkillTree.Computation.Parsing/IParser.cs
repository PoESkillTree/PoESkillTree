using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Generic interface for parsing stat lines.
    /// </summary>
    /// <typeparam name="TResult">The type of parsing results</typeparam>
    public interface IParser<TResult>
    {
        /// <summary>
        /// Returns true and outputs the parsing <c>result</c> if <c>stat</c> could be parsed.
        /// </summary>
        /// <param name="stat">the stat line that should be parsed</param>
        /// <param name="remaining">the parts of <c>stat</c> that were not parsed into <c>result</c>.</param>
        /// <param name="result">the parsing result. May be null if the function returns false.</param>
        /// <returns>True if <c>stat</c> was parsed successfully and completely.</returns>
        /// <remarks>
        /// If false is returned, <c>result</c> is undefined and should only be used for debugging purposes
        /// (it may be a partial result containing null properties, it may be null).
        /// <para>
        /// Throws <see cref="ParseException"/> if the data specification is erroneous, e.g. it tries to reference
        /// values that don't occur in the matched stat. You'll want to handle stats that throw a
        /// <see cref="ParseException"/> on being parsed like unparsable stats.
        /// </para>
        /// </remarks>
        bool TryParse(string stat, out string remaining, out TResult result);
    }

    /// <summary>
    /// This is the main interface for using Computation.Parsing. It parses stat lines to <see cref="Modifier"/>s.
    /// </summary>
    public interface IParser : IParser<IReadOnlyList<Modifier>>
    {
        /// <summary>
        /// Returns true and outputs the parsed <see cref="Modifier"/>s if <c>stat</c> could be parsed.
        /// </summary>
        /// <param name="stat">the stat line that should be parsed</param>
        /// <param name="remaining">the parts of <c>stat</c> that were not parsed into <c>result</c>. 
        /// Empty if true is returned.</param>
        /// <param name="result">the parsing result. May be null if the function returns false.</param>
        /// <returns>True if <c>stat</c> was parsed successfully and completely.</returns>
        /// <remarks>
        /// If false is returned, <c>result</c> is undefined and should only be used for debugging purposes
        /// (it may contain partial <see cref="Modifier"/>s with null properties, it may be null).
        /// <para>
        /// Throws <see cref="ParseException"/> if the data specification is erroneous, e.g. it tries to reference
        /// values that don't occur in the matched stat. You'll want to handle stats that throw a
        /// <see cref="ParseException"/> on being parsed like unparsable stats.
        /// </para>
        /// </remarks>
        new bool TryParse(string stat, out string remaining, out IReadOnlyList<Modifier> result);
    }
}