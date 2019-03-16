namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <summary>
    /// Generic interface for parsing modifier lines.
    /// </summary>
    /// <typeparam name="TResult">The type of parsing results.</typeparam>
    public interface IStringParser<TResult>
    {
        StringParseResult<TResult> Parse(string modifierLine);
    }
}