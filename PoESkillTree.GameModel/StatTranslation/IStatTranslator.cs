using System.Collections.Generic;
using System.Linq;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.StatTranslation
{
    public interface IStatTranslator
    {
        StatTranslatorResult Translate(IEnumerable<UntranslatedStat> untranslatedStats);
    }

    public class StatTranslatorResult
    {
        public StatTranslatorResult(
            IReadOnlyList<string> translatedStats, IReadOnlyList<UntranslatedStat> unknownStats)
            => (TranslatedStats, UnknownStats) = (translatedStats, unknownStats);

        public IReadOnlyList<string> TranslatedStats { get; }
        public IReadOnlyList<UntranslatedStat> UnknownStats { get; }

        public override bool Equals(object obj)
            => obj == this || (obj is StatTranslatorResult other && Equals(other));

        private bool Equals(StatTranslatorResult other)
            => TranslatedStats.SequenceEqual(other.TranslatedStats) && UnknownStats.SequenceEqual(other.UnknownStats);

        public override int GetHashCode()
            => (TranslatedStats.SequenceHash(), UnknownStats.SequenceHash()).GetHashCode();
    }
}