using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Parsing
{
    public static class ParserExtensions
    {
        public static ParseResult Parse(this IParser parser, string stat, IModifierSource originalSource)
        {
            var innerResult = parser.Parse(stat);
            var modifiers = innerResult.Result.Select(UpdateSource).ToList();
            return new ParseResult(innerResult.SuccessfullyParsed, innerResult.RemainingStat, modifiers);

            Modifier UpdateSource(Modifier modifier) =>
                modifier.Source.FirstLevel != ModifierSourceFirstLevel.Global
                    ? modifier
                    : new Modifier(modifier.Stats, modifier.Form, modifier.Value, originalSource);
        }
    }
}