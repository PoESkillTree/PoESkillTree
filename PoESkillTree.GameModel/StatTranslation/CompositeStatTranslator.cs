using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.StatTranslation
{
    public class CompositeStatTranslator : IStatTranslator
    {
        private readonly IReadOnlyList<IStatTranslator> _components;

        public CompositeStatTranslator(IReadOnlyList<IStatTranslator> components)
            => _components = components;

        public StatTranslatorResult Translate(IEnumerable<UntranslatedStat> untranslatedStats)
        {
            var translatedStats = new List<string>();
            var unknownStats = untranslatedStats.ToList();
            foreach (var translator in _components)
            {
                var result = translator.Translate(unknownStats);
                translatedStats.AddRange(result.TranslatedStats);
                unknownStats = result.UnknownStats.ToList();
            }
            return new StatTranslatorResult(translatedStats, unknownStats);
        }
    }
}