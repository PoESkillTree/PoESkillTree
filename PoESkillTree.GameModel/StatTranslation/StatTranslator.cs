using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using MoreLinq;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Takes the deserialized stat_translations and returns Translation objects for given stat ids.
    /// </summary>
    public class StatTranslator : IStatTranslator
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StatTranslator));

        private readonly Lazy<ILookup<string, Translation>> _translationLookup;

        /// <param name="jsonTranslations">the deserialized stat_translations to use as basis for translations</param>
        public StatTranslator(IEnumerable<JsonStatTranslation> jsonTranslations)
        {
            _translationLookup =
                new Lazy<ILookup<string, Translation>>(() => CreateTranslationLookup(jsonTranslations));
        }

        private static ILookup<string, Translation> CreateTranslationLookup(
            IEnumerable<JsonStatTranslation> jsonTranslations)
        {
            // ids are not unique over all translations: (-> must be lookup and not dictionary)
            // there are 2 cases where an id appears once alone and once together with another id
            return (
                from t in jsonTranslations
                let translation = new Translation(t)
                from id in t.Ids
                select new { id, translation }
            ).ToLookup(x => x.id, x => x.translation);
        }

        /// <summary>
        /// Returns the Translation objects matching the given stat ids. Each Translation appears only once.
        /// </summary>
        public IEnumerable<Translation> GetTranslations(IEnumerable<string> statIds)
        {
            var ids = statIds.ToHashSet();
            var translations = new List<Translation>();
            foreach (var id in ids)
            {
                if (!_translationLookup.Value.Contains(id))
                {
                    Log.Warn("Unknown stat id: " + id);
                    continue;
                }
                var ts = _translationLookup.Value[id].ToList();
                if (ts.Count > 1)
                {
                    // For the 2 cases where there is more than one translation in the lookup, 
                    // take the full match with the most ids (the one with 2 ids if both are given, else
                    // the one with only the current id).
                    var fullMatches = ts.Where(t => t.Ids.All(ids.Contains));
                    translations.Add(fullMatches.MaxBy(t => t.Ids.Count));
                }
                else
                {
                    // There is at least one translation in the lookup for each id
                    translations.Add(ts[0]);
                }
            }
            return translations.Distinct();
        }

        /// <summary>
        /// Returns the translated strings for the given stat ids and values.
        /// </summary>
        public IEnumerable<string> GetTranslations(IReadOnlyDictionary<string, int> idValueDict)
            => GetTranslations(idValueDict.Keys).Select(t => t.Translate(idValueDict));

        public IEnumerable<string> Translate(IEnumerable<UntranslatedStat> untranslatedStats)
            => GetTranslations(untranslatedStats.ToDictionary(s => s.StatId, s => s.Value));
    }
}