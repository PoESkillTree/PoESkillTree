using System.Collections.Generic;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that caches results.
    /// </summary>
    public class CachingParser<T> : IParser<T>
    {
        private readonly IParser<T> _decoratedParser;

        private readonly Dictionary<string, ParseResult<T>> _cache = new Dictionary<string, ParseResult<T>>();

        public CachingParser(IParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public ParseResult<T> Parse(string stat)
        {
            return _cache.GetOrAdd(stat, _decoratedParser.Parse);
        }
    }
}