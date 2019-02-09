using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that normalizes the input stat before passing it to the decorated parser.
    /// <para>Normalizing means converting trimming and replacing all whitespace sequences by a single space.
    /// </para>
    /// </summary>
    public class StatNormalizingParser<TResult> : IStringParser<TResult>
    {
        private readonly IStringParser<TResult> _inner;

        public StatNormalizingParser(IStringParser<TResult> inner)
        {
            _inner = inner;
        }

        public StringParseResult<TResult> Parse(CoreParserParameter parameter)
        {
            var stat = parameter.ModifierLine;
            var processed = Regex.Replace(stat.Trim(), @"\s+", " ");
            return _inner.Parse(processed, parameter);
        }
    }
}