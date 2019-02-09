using System;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that applies a function to the decorated parser's results.
    /// </summary>
    /// <typeparam name="TSource">Type of the decorated parser's results</typeparam>
    /// <typeparam name="TResult">Type of this parser's results</typeparam>
    public class ResultMappingParser<TSource, TResult> : IStringParser<TResult>
    {
        private readonly IStringParser<TSource> _inner;
        private readonly Func<CoreParserParameter, TSource, TResult> _mapper;

        public ResultMappingParser(
            IStringParser<TSource> inner, Func<CoreParserParameter, TSource, TResult> mapper)
        {
            _inner = inner;
            _mapper = mapper;
        }

        public StringParseResult<TResult> Parse(CoreParserParameter parameter)
        {
            var (successfullyParsed, remaining, innerResult) = _inner.Parse(parameter);
            return (successfullyParsed, remaining, _mapper(parameter, innerResult));
        }
    }
}