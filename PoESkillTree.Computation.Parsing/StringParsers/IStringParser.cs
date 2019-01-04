using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Parsing.StringParsers
{
    /// <summary>
    /// Generic interface for parsing modifier lines.
    /// </summary>
    /// <typeparam name="TResult">The type of parsing results.</typeparam>
    public interface IStringParser<TResult>
    {
        StringParseResult<TResult> Parse(CoreParserParameter parameter);
    }

    public static class StringParserExtensions
    {
        public static StringParseResult<TResult> Parse<TResult>(this IStringParser<TResult> @this,
            string modifierLine, CoreParserParameter previousParameter)
            => @this.Parse(modifierLine, previousParameter.ModifierSource, previousParameter.ModifierSourceEntity);

        public static StringParseResult<TResult> Parse<TResult>(this IStringParser<TResult> @this,
            string modifierLine, ModifierSource modifierSource, Entity modifierSourceEntity)
        {
            var parameter = new CoreParserParameter(modifierLine, modifierSource, modifierSourceEntity);
            return @this.Parse(parameter);
        }
    }
}