using System;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that applies a function to the decorated parser's results.
    /// </summary>
    /// <typeparam name="TSource">Type of the decorated paser's results</typeparam>
    /// <typeparam name="TResult">Type of this parser's results</typeparam>
    public class ResultMappingParser<TSource, TResult> : IParser<TResult>
    {
        private readonly IParser<TSource> _inner;
        private readonly Func<TSource, TResult> _mapper;

        public ResultMappingParser(IParser<TSource> inner, Func<TSource, TResult> mapper)
        {
            _inner = inner;
            _mapper = mapper;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            var ret = _inner.TryParse(stat, out remaining, out var source);
            result = _mapper(source);
            return ret;
        }
    }
}