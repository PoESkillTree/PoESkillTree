using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing
{
    public class CachingParser<T> : IParser<T>
    {
        private readonly IParser<T> _decoratedParser;

        private readonly Dictionary<string, (bool ret, string remaining, T result)> _cache =
            new Dictionary<string, (bool ret, string remaining, T result)>();

        public CachingParser(IParser<T> decoratedParser)
        {
            _decoratedParser = decoratedParser;
        }

        public bool TryParse(string stat, out string remaining, out T result)
        {
            if (_cache.TryGetValue(stat, out var cached))
            {
                result = cached.result;
                remaining = cached.remaining;
                return cached.ret;
            }
            var ret = _decoratedParser.TryParse(stat, out remaining, out result);
            _cache[stat] = (ret, remaining, result);
            return ret;
        }
    }
}