using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MoreLinq;
using PoESkillTree.GameModel.Logging;

namespace PoESkillTree.GameModel.StatTranslation
{
    /// <summary>
    /// Takes the deserialized stat_translations and returns Translation objects for given stat ids.
    /// </summary>
    public class StatTranslator : IStatTranslator
    {
        private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

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

        public StatTranslatorResult Translate(IEnumerable<UntranslatedStat> untranslatedStats)
        {
            var idStatDict = untranslatedStats
                .GroupBy(s => s.StatId)
                .ToDictionary(g => g.Key, s => s.Aggregate(Merge));
            var (translations, unknownStatIds) = LookupStatIds(idStatDict.Keys);
            var unknownStats = unknownStatIds.Select(k => idStatDict[k]);

            var idValueDict = idStatDict.ToDictionary(p => p.Key, p => p.Value.Value);
            var translatedStats = translations.Select(t => t.Translate(idValueDict)).Where(s => s != null);

            return new StatTranslatorResult(translatedStats.ToList(), unknownStats.ToList());

            UntranslatedStat Merge(UntranslatedStat left, UntranslatedStat right)
                => new UntranslatedStat(left.StatId, left.Value + right.Value);
        }

        /// <summary>
        /// Returns the translated strings for the given stat ids and values.
        /// </summary>
        [ItemCanBeNull]
        public IEnumerable<string> GetTranslations(IReadOnlyDictionary<string, int> idValueDict)
            => GetTranslations(idValueDict.Keys).Select(t => t.Translate(idValueDict));

        /// <summary>
        /// Returns the Translation objects matching the given stat ids. Each Translation appears only once.
        /// </summary>
        public IEnumerable<Translation> GetTranslations(IEnumerable<string> statIds)
        {
            var (translations, unknownStatIds) = LookupStatIds(statIds);
            if (unknownStatIds.Any())
            {
                Log.Warn("Unknown stat ids: " + unknownStatIds.ToDelimitedString(","));
            }
            return translations;
        }

        private (IEnumerable<Translation> translations, IReadOnlyList<string> unknownStatIds)
            LookupStatIds(IEnumerable<string> statIds)
        {
            var ids = statIds.ToHashSet();
            var translations = new List<Translation>();
            var unknownStatIds = new List<string>();
            foreach (var id in ids)
            {
                if (!_translationLookup.Value.Contains(id))
                {
                    unknownStatIds.Add(id);
                    continue;
                }
                var ts = _translationLookup.Value[id].ToList();
                if (ts.Count > 1)
                {
                    // For the 2 cases where there is more than one translation in the lookup, 
                    // take the full match with the most ids (the one with 2 ids if both are given, else
                    // the one with only the current id).
                    var fullMatches = ts.Where(t => t.Ids.All(ids.Contains));
                    translations.Add(fullMatches.MaxBy(t => t.Ids.Count).First());
                }
                else
                {
                    // There is at least one translation in the lookup for each id
                    translations.Add(ts[0]);
                }
            }
            return (translations.Distinct(), unknownStatIds);
        }
    }
}