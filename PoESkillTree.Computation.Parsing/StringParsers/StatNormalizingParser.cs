using System.Collections.Generic;

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
            var modifierLine = MergeWhiteSpace(parameter.ModifierLine.Trim());
            return _inner.Parse(modifierLine, parameter);
        }

        // This is infinitely faster than Regex.Replace
        private static string MergeWhiteSpace(string s)
        {
            var result = new List<char>(s.Length);
            var lastWasWhiteSpace = true;
            foreach (var c in s)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!lastWasWhiteSpace)
                    {
                        result.Add(' ');
                    }
                    lastWasWhiteSpace = true;
                }
                else
                {
                    result.Add(c);
                    lastWasWhiteSpace = false;
                }
            }
            return new string(result.ToArray());
        }
    }
}