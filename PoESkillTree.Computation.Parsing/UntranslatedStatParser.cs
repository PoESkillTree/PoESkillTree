using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.GameModel.StatTranslation;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing
{
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

    public class UntranslatedStatParserParameter
    {
        public UntranslatedStatParserParameter(
            ModifierSource.Local.Skill localModifierSource, IEnumerable<UntranslatedStat> untranslatedStats)
            => (LocalModifierSource, ModifierSourceEntity, UntranslatedStats) =
                (localModifierSource, default, untranslatedStats);

        public UntranslatedStatParserParameter(
            ModifierSource.Local.Skill localModifierSource, Entity modifierSourceEntity,
            IEnumerable<UntranslatedStat> untranslatedStats)
            => (LocalModifierSource, ModifierSourceEntity, UntranslatedStats) =
                (localModifierSource, modifierSourceEntity, untranslatedStats);

        public ModifierSource.Local.Skill LocalModifierSource { get; }
        public Entity ModifierSourceEntity { get; }
        public IEnumerable<UntranslatedStat> UntranslatedStats { get; }

        public override bool Equals(object obj)
            => (obj == this) || (obj is UntranslatedStatParserParameter other && Equals(other));

        private bool Equals(UntranslatedStatParserParameter other)
            => LocalModifierSource == other.LocalModifierSource &&
               ModifierSourceEntity == other.ModifierSourceEntity &&
               UntranslatedStats.SequenceEqual(other.UntranslatedStats);

        public override int GetHashCode()
            => (LocalModifierSource, ModifierSourceEntity, UntranslatedStats.SequenceHash()).GetHashCode();
    }
}