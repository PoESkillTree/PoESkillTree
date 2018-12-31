using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Translates <see cref="UntranslatedStat"/>s into natural language.
    /// </summary>
    public interface IStatTranslator
    {
        StatTranslatorResult Translate(IEnumerable<UntranslatedStat> untranslatedStats);
    }

    public class StatTranslatorResult : ValueObject
    {
        public StatTranslatorResult(
            IReadOnlyList<string> translatedStats, IReadOnlyList<UntranslatedStat> unknownStats)
            => (TranslatedStats, UnknownStats) = (translatedStats, unknownStats);

        public IReadOnlyList<string> TranslatedStats { get; }
        public IReadOnlyList<UntranslatedStat> UnknownStats { get; }

        protected override object ToTuple()
            => (WithSequenceEquality(TranslatedStats), WithSequenceEquality(UnknownStats));
    }
}