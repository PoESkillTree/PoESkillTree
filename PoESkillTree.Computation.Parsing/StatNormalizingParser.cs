using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that normalizes the input stat before passing it to the decorated parser.
    /// <para>Normalizing means converting trimming and replacing all whitespace sequences by a single space.
    /// </para>
    /// </summary>
    public class StatNormalizingParser<TResult> : IParser<TResult>
    {
        private readonly IParser<TResult> _inner;

        public StatNormalizingParser(IParser<TResult> inner)
        {
            _inner = inner;
        }

        public ParseResult<TResult> Parse(string stat)
        {
            var processed = Regex.Replace(stat.Trim(), @"\s+", " ");
            return _inner.Parse(processed);
        }
    }
}