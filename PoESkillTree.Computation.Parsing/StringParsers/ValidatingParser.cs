using System;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that makes sure stats were parsed completely and <see cref="ParseResult{T}.RemainingStat"/>
    /// is always empty when <see cref="ParseResult{T}.SuccessfullyParsed"/> is true.
    /// It removes <see cref="ItemConstants.HiddenStatSuffix"/> from the decorated parser's remaining and trims it.
    /// If remaining is still not empty, <see cref="Parse"/> sets <see cref="ParseResult{T}.SuccessfullyParsed"/> to
    /// false even if the decorated parser's was true.
    /// </summary>
    public class ValidatingParser<TResult> : IStringParser<TResult>
    {
        private readonly IStringParser<TResult> _inner;

        public ValidatingParser(IStringParser<TResult> inner)
        {
            _inner = inner;
        }

        public StringParseResult<TResult> Parse(string modifierLine)
        {
            var (successfullyParsed, remaining, result) = _inner.Parse(modifierLine);

            if (remaining.EndsWith(ItemConstants.HiddenStatSuffix, StringComparison.OrdinalIgnoreCase))
            {
                remaining = remaining.Remove(remaining.Length - ItemConstants.HiddenStatSuffix.Length);
            }

            remaining = remaining.Trim();
            return (successfullyParsed && remaining.Length == 0, remaining, result);
        }
    }
}