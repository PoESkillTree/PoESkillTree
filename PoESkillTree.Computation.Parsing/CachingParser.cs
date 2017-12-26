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

        private readonly Dictionary<string, CacheItem> _cache = new Dictionary<string, CacheItem>();

        public CachingParser(IParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public bool TryParse(string stat, out string remaining, out T result)
        {
            var item = _cache.GetOrAdd(stat, CacheMiss);
            remaining = item.Remaining;
            result = item.Result;
            return item.ReturnValue;
        }

        private CacheItem CacheMiss(string stat)
        {
            var returnValue = _decoratedParser.TryParse(stat, out var remaining, out var result);
            return new CacheItem(returnValue, remaining, result);
        }


        private struct CacheItem
        {
            public bool ReturnValue { get; }
            public string Remaining { get; }
            public T Result { get; }

            public CacheItem(bool returnValue, string remaining, T result)
            {
                ReturnValue = returnValue;
                Remaining = remaining;
                Result = result;
            }
        }
    }
}