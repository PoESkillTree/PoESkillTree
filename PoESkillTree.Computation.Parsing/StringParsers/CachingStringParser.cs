namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <summary>
    /// Decorating parser that caches results.
    /// </summary>
    public class CachingStringParser<T>
        : GenericCachingParser<string, StringParseResult<T>>, IStringParser<T>
    {
        public CachingStringParser(IStringParser<T> decoratedParser)
            : base(decoratedParser.Parse)
        {
        }
    }
}