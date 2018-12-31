using System.Collections.Generic;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that caches results.
    /// </summary>
    public class CachingParser<T> : IStringParser<T>
    {
        private readonly IStringParser<T> _decoratedParser;

        private readonly Dictionary<string, StringParseResult<T>> _cache =
            new Dictionary<string, StringParseResult<T>>();

        public CachingParser(IStringParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public StringParseResult<T> Parse(string stat)
        {
            return _cache.GetOrAdd(stat, stat1 => _decoratedParser.Parse(stat1));
        }
    }
}