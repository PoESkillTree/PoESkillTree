using System.Collections.Concurrent;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that caches results.
    /// </summary>
    public class CachingParser<T> : IStringParser<T>
    {
        private readonly IStringParser<T> _decoratedParser;

        private readonly ConcurrentDictionary<CoreParserParameter, StringParseResult<T>> _cache =
            new ConcurrentDictionary<CoreParserParameter, StringParseResult<T>>();

        public CachingParser(IStringParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public StringParseResult<T> Parse(CoreParserParameter parameter)
        {
            return _cache.GetOrAdd(parameter, _decoratedParser.Parse);
        }
    }
}