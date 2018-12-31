using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// This is the core interface for using Computation.Parsing. It parses modifier lines to <see cref="Modifier"/>s.
    /// </summary>
    public interface ICoreParser : IParser<CoreParserParameter>
    {
    }

    public class CoreParserParameter : ValueObject
    {
        public CoreParserParameter(string modifierLine, ModifierSource modifierSource, Entity modifierSourceEntity)
            => (ModifierLine, ModifierSource, ModifierSourceEntity) =
                (modifierLine, modifierSource, modifierSourceEntity);

        /// <summary>
        /// The modifier line that should be parsed.
        /// </summary>
        public string ModifierLine { get; }

        /// <summary>
        /// The source of the modifiers.
        /// </summary>
        public ModifierSource ModifierSource { get; }

        /// <summary>
        /// The entity type of the modifier source.
        /// </summary>
        public Entity ModifierSourceEntity { get; }

        protected override object ToTuple() => (ModifierLine, ModifierSource, ModifierSourceEntity);
    }

    public static class CoreParserExtensions
    {
        public static ParseResult Parse(this ICoreParser @this,
            string modifierLine, ModifierSource modifierSource, Entity modifierSourceEntity)
        {
            var parameter = new CoreParserParameter(modifierLine, modifierSource, modifierSourceEntity);
            return @this.Parse(parameter);
        }
    }
}