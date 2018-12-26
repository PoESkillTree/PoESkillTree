using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Parser that translates and parses <see cref="UntranslatedStat"/>s
    /// </summary>
    public class UntranslatedStatParser : IParser<UntranslatedStatParserParameter>
    {
        private readonly IStatTranslator _translator;
        private readonly ICoreParser _coreParser;

        public UntranslatedStatParser(IStatTranslator translator, ICoreParser coreParser)
            => (_translator, _coreParser) = (translator, coreParser);

        public ParseResult Parse(UntranslatedStatParserParameter parameter)
        {
            var modifierLines = _translator.Translate(parameter.UntranslatedStats).TranslatedStats;
            var modifierSource = new ModifierSource.Global(parameter.LocalModifierSource);
            var entity = parameter.ModifierSourceEntity;
            var parseResults = modifierLines.Select(m => _coreParser.Parse(m, modifierSource, entity));
            return ParseResult.Aggregate(parseResults);
        }
    }

    public static class UntranslatedStatParserExtensions
    {
        public static ParseResult Parse(this IParser<UntranslatedStatParserParameter> @this,
            ModifierSource.Local localModifierSource, Entity modifierSourceEntity,
            IReadOnlyList<UntranslatedStat> untranslatedStats)
            => @this.Parse(new UntranslatedStatParserParameter(localModifierSource, modifierSourceEntity,
                untranslatedStats));
    }

    public class UntranslatedStatParserParameter : ValueObject
    {
        public UntranslatedStatParserParameter(
            ModifierSource.Local localModifierSource, IReadOnlyList<UntranslatedStat> untranslatedStats)
            => (LocalModifierSource, ModifierSourceEntity, UntranslatedStats) =
                (localModifierSource, default, untranslatedStats);

        public UntranslatedStatParserParameter(
            ModifierSource.Local localModifierSource, Entity modifierSourceEntity,
            IReadOnlyList<UntranslatedStat> untranslatedStats)
            => (LocalModifierSource, ModifierSourceEntity, UntranslatedStats) =
                (localModifierSource, modifierSourceEntity, untranslatedStats);

        public ModifierSource.Local LocalModifierSource { get; }
        public Entity ModifierSourceEntity { get; }
        public IReadOnlyList<UntranslatedStat> UntranslatedStats { get; }

        protected override object ToTuple()
            => (LocalModifierSource, ModifierSourceEntity, WithSequenceEquality(UntranslatedStats));
    }
}