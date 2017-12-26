using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Decorating parser that normalizes the input stat before passing it to the decorated parser.
    /// <para>Normalizing means converting to lower case, trimming and replacing all whitespace sequences by a single
    /// space.
    /// </para>
    /// </summary>
    public class StatNormalizingParser<TResult> : IParser<TResult>
    {
        private readonly IParser<TResult> _inner;

        public StatNormalizingParser(IParser<TResult> inner)
        {
            _inner = inner;
        }

        public bool TryParse(string stat, out string remaining, out TResult result)
        {
            var processed = stat.ToLowerInvariant().Trim();
            processed = Regex.Replace(processed, @"\s+", " ");
            return _inner.TryParse(processed, out remaining, out result);
        }
    }
}