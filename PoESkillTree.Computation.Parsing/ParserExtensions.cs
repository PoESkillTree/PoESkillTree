using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Parsing
{
    public static class ParserExtensions
    {
        /// <summary>
        /// Calls <see cref="IParser.Parse"/> and sets <see cref="Modifier.Source"/> to the given source in all cases
        /// where it was not overridden by parsing.
        /// </summary>
        public static ParseResult Parse(this IParser parser, string stat, ModifierSource originalSource)
        {
            var innerResult = parser.Parse(stat);
            var modifiers = innerResult.Result.Select(UpdateSource).ToList();
            return new ParseResult(innerResult.SuccessfullyParsed, innerResult.RemainingStat, modifiers);

            Modifier UpdateSource(Modifier modifier) =>
                modifier.Source is ModifierSource.Global
                    ? new Modifier(modifier.Stats, modifier.Form, modifier.Value, originalSource)
                    : modifier;
        }
    }
}