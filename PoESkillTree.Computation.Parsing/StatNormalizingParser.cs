using System.Text.RegularExpressions;

namespace PoESkillTree.Computation.Parsing
{
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