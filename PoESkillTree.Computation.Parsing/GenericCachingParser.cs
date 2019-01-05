using System;
using System.Collections.Concurrent;

namespace PoESkillTree.Computation.Parsing
{
    public abstract class GenericCachingParser<TParameter, TResult>
    {
        private readonly Func<TParameter, TResult> _parser;

        private readonly ConcurrentDictionary<TParameter, TResult> _cache =
            new ConcurrentDictionary<TParameter, TResult>();

        protected GenericCachingParser(Func<TParameter, TResult> parser)
            => _parser = parser;

        public TResult Parse(TParameter parameter)
            => _cache.GetOrAdd(parameter, _parser);
    }
}