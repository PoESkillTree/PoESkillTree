using System;

namespace PoESkillTree.Computation.Parsing
{
    public class ParserWithResultSelector<TSource, TResult> : IParser<TResult>
    {
        private readonly IParser<TSource> _inner;
        private readonly Func<TSource, TResult> _selector;

        public ParserWithResultSelector(IParser<TSource> inner, Func<TSource, TResult> selector)
        {
            _inner = inner;
            _selector = selector;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            var ret = _inner.TryParse(stat, out remaining, out var source);
            result = _selector(source);
            return ret;
        }
    }
}