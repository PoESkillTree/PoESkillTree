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

        public StringParseResult<TResult> Parse(string modifierLine)
        {
            var normalized = StringNormalizer.MergeWhiteSpace(modifierLine.Trim());
            return _inner.Parse(normalized);
        }
    }
}