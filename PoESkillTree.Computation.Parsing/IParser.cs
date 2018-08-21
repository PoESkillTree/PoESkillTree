using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// This is the main interface for using Computation.Parsing. It parses stat lines to <see cref="Modifier"/>s.
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parses the given stat line into <see cref="Modifier"/>.
        /// </summary>
        /// <param name="stat">the stat line that should be parsed</param>
        /// <param name="modifierSource">the source of the modifier</param>
        /// <param name="modifierSourceEntity">the entity type of the modifier source</param>
        /// <remarks>
        /// <para> If <paramref name="stat"/> was parsed successfully and completely, the return value's 
        /// <see cref="ParseResult.SuccessfullyParsed"/> is true, <see cref="ParseResult.Result"/>
        /// contains the parsing result and <see cref="ParseResult.RemainingStat"/> is empty. </para>
        /// <para>If <paramref name="stat"/> could not be parsed, <see cref="ParseResult.SuccessfullyParsed"/> is
        /// false and <see cref="ParseResult.Result"/> only contains <see cref="Modifier"/>s for which a stat, form
        /// and value could be parsed.</para>
        /// <para><see cref="ParseResult.RemainingStat"/> contains the parts of <paramref name="stat"/> that were
        /// not parsed into <see cref="ParseResult.Result"/>.</para>
        /// <para>
        /// Throws <see cref="Common.Parsing.ParseException"/> if the data specification is erroneous, e.g. it tries to
        /// reference values that don't occur in the matched stat. You'll want to handle stats that throw a
        /// <see cref="Common.Parsing.ParseException"/> on being parsed like not parsable stats.
        /// </para>
        /// </remarks>
        ParseResult Parse(string stat, ModifierSource modifierSource, Entity modifierSourceEntity);
    }
}