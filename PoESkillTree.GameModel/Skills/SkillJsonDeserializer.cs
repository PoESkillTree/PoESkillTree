using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumsNET;
using MoreLinq;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.GameModel.Skills
{
    /// <summary>
    /// Deserializes <see cref="SkillDefinitions"/> from RePoE's gems and gem_tooltips and extends that data using
    /// <see cref="SkillDefinitionExtensions"/>.
    /// </summary>
    public class SkillJsonDeserializer
    {
        private readonly SkillDefinitionExtensions _definitionExtensions;
        private int _nextNumericId;
        private SkillDefinitionExtension _definitionExtension;

        private SkillJsonDeserializer(SkillDefinitionExtensions definitionExtensions)
            => _definitionExtensions = definitionExtensions;

        public static async Task<SkillDefinitions> DeserializeAsync(bool deserializeOnThreadPool)
        {
            var gemJsonTask = DataUtils.LoadRePoEAsObjectAsync("gems", deserializeOnThreadPool);
            var gemTooltipJsonTask = DataUtils.LoadRePoEAsObjectAsync("gem_tooltips", deserializeOnThreadPool);
            return Deserialize(
                await gemJsonTask.ConfigureAwait(false),
                await gemTooltipJsonTask.ConfigureAwait(false));
        }

        public static SkillDefinitions Deserialize(JObject gemJson, JToken gemTooltipJson)
        {
            var deserializer = new SkillJsonDeserializer(new SkillDefinitionExtensions());
            var definitions = gemJson.Properties()
                .Select(property => deserializer.Deserialize(property, gemTooltipJson))
                .Where(d => d != null);
            return new SkillDefinitions(definitions.ToList());
        }

        private SkillDefinition Deserialize(JProperty gemProperty, JToken gemTooltipJson)
            => Deserialize(gemProperty.Name, (JObject) gemProperty.Value,
                gemTooltipJson.Value<JObject>(gemProperty.Name));

        private SkillDefinition Deserialize(string skillId, JObject gemJson, JToken gemTooltipJson)
        {
            var castTime = gemJson.Value<int>("cast_time");
            var statTranslationFile = gemJson.Value<string>("stat_translation_file");

            var baseItemJson = gemJson["base_item"];
            ISet<string> gemTags;
            SkillBaseItemDefinition baseItemDefinition;
            if (baseItemJson.Type == JTokenType.Null)
            {
                gemTags = new HashSet<string>();
                baseItemDefinition = null;
            }
            else
            {
                gemTags = gemJson["tags"].Values<string>().ToHashSet();
                var releaseState = Enums.Parse<ReleaseState>(baseItemJson.Value<string>("release_state"), true);
                if (releaseState == ReleaseState.Unreleased)
                    return null;
                baseItemDefinition = new SkillBaseItemDefinition(
                    baseItemJson.Value<string>("display_name"),
                    baseItemJson.Value<string>("id"),
                    releaseState,
                    gemTags);
            }

            var numericId = _nextNumericId;
            _nextNumericId++;
            _definitionExtension = _definitionExtensions.GetExtensionForSkill(skillId);
            var levels = DeserializeLevels(gemJson, gemTooltipJson);

            var displayName = baseItemDefinition?.DisplayName;
            if (gemJson.TryGetValue("active_skill", out var activeSkillJson))
            {
                displayName = displayName ?? activeSkillJson.Value<string>("display_name");
                var activeSkillTypes = activeSkillJson["types"].Values<string>().ToHashSet();
                var minionActiveSkillTypes = activeSkillJson["minion_types"]?.Values<string>().ToHashSet()
                                             ?? new HashSet<string>();
                var keywords = GetKeywords(displayName, activeSkillTypes, gemTags);
                var providesBuff = _definitionExtension.BuffStats.Any();
                var totemLifeMultiplier = activeSkillJson.Value<double?>("skill_totem_life_multiplier");
                var weaponRestrictions = activeSkillJson["weapon_restrictions"].Values<string>()
                    .Select(ItemClassEx.Parse).ToList();
                var activeSkillDefinition = new ActiveSkillDefinition(
                    displayName, castTime, activeSkillTypes, minionActiveSkillTypes, keywords,
                    GetKeywordsPerPart(keywords), providesBuff, totemLifeMultiplier, weaponRestrictions);
                return SkillDefinition.CreateActive(skillId, numericId, statTranslationFile,
                    _definitionExtension.PartNames, baseItemDefinition, activeSkillDefinition, levels);
            }
            else
            {
                displayName = displayName ?? skillId;
                var supportSkillJson = gemJson["support_gem"];
                var addedActiveSkillTypes = supportSkillJson["added_types"].Values<string>().ToHashSet();
                var addedKeywords = GetKeywords(displayName, addedActiveSkillTypes, gemTags);
                var supportSkillDefinition = new SupportSkillDefinition(
                    supportSkillJson.Value<bool>("supports_gems_only"),
                    supportSkillJson["allowed_types"].Values<string>().ToList(),
                    supportSkillJson["excluded_types"].Values<string>().ToList(),
                    addedActiveSkillTypes, addedKeywords);
                return SkillDefinition.CreateSupport(skillId, numericId, statTranslationFile,
                    _definitionExtension.PartNames, baseItemDefinition, supportSkillDefinition, levels);
            }
        }

        private IReadOnlyList<Keyword> GetKeywords(
            string displayName, ISet<string> activeSkillTypes, ISet<string> gemTags)
        {
            var keywords = Enums.GetValues<Keyword>()
                .Where(k => k.IsOnSkill(displayName, activeSkillTypes, gemTags));
            return _definitionExtension.CommonExtension.ModifyKeywords(keywords).ToList();
        }

        private IReadOnlyList<IReadOnlyList<Keyword>> GetKeywordsPerPart(IReadOnlyList<Keyword> keywords)
            => _definitionExtension.PartExtensions.Select(e => e.ModifyKeywords(keywords).ToList()).ToList();

        private IReadOnlyDictionary<int, SkillLevelDefinition> DeserializeLevels(JToken gemJson, JToken gemTooltipJson)
        {
            var staticJson = gemJson["static"];
            var staticTooltipJson = gemTooltipJson["static"];
            var perLevel = gemJson["per_level"];
            var levels = new Dictionary<int, SkillLevelDefinition>();
            foreach (var perLevelProp in perLevel.Cast<JProperty>())
            {
                var tooltip = DeserializeTooltip(gemTooltipJson["per_level"][perLevelProp.Name], staticTooltipJson);
                levels[int.Parse(perLevelProp.Name)] =
                    DeserializeLevel(perLevelProp.Value, staticJson, tooltip);
            }
            return levels;
        }

        private SkillLevelDefinition DeserializeLevel(
            JToken levelJson, JToken staticJson, SkillTooltipDefinition tooltip)
        {
            var allQualityStats = Stats("quality_stats");
            var (nonBuffQualityStats, qualityBuffStats) = SplitBuffStats(allQualityStats);
            var (qualityStats, qualityPassiveStats) = SplitPassiveStats(nonBuffQualityStats);

            var allStats = Stats("stats");
            var (nonBuffStats, buffStats) = SplitBuffStats(allStats);
            var (nonPassiveStats, passiveStats) = SplitPassiveStats(nonBuffStats);
            var (commonStats, additionalStatsPerPart) = SplitStatsIntoParts(nonPassiveStats);

            return new SkillLevelDefinition(
                Value<int?>("damage_effectiveness") / 100D + 1,
                Value<int?>("damage_multiplier") / 10000D + 1,
                Value<int?>("crit_chance") / 100D,
                Value<int?>("attack_speed_multiplier"),
                Value<int?>("mana_cost"),
                Value<int?>("mana_multiplier") / 100D,
                Value<int?>("mana_reservation_override"),
                Value<int?>("cooldown"),
                Value<int>("required_level"),
                NestedValue<int>("stat_requirements", "dex"),
                NestedValue<int>("stat_requirements", "int"),
                NestedValue<int>("stat_requirements", "str"),
                qualityStats,
                commonStats,
                additionalStatsPerPart,
                qualityBuffStats,
                buffStats,
                qualityPassiveStats,
                passiveStats,
                tooltip);

            T NestedValue<T>(string propertyName, string nestedPropertyName)
                => GetValue<T>(nestedPropertyName, levelJson[propertyName], staticJson[propertyName]);

            T Value<T>(string propertyName) => GetValue<T>(propertyName, levelJson, staticJson);

            IReadOnlyList<UntranslatedStat> Stats(string propertyName)
            {
                var stats = DeserializeStats(
                    Value<JArray>(propertyName), staticJson.Value<JArray>(propertyName), DeserializeUntranslatedStat);
                return _definitionExtension.CommonExtension.ModifyStats(stats).ToList();
            }
        }

        private static UntranslatedStat DeserializeUntranslatedStat(JToken statToken, JToken staticStatToken)
            => new UntranslatedStat(
                GetValue<string>("id", statToken, staticStatToken),
                GetValue<int>("value", statToken, staticStatToken));

        private (IReadOnlyList<UntranslatedStat>, IReadOnlyList<BuffStat>) SplitBuffStats(
            IReadOnlyList<UntranslatedStat> stats)
        {
            var untranslatedBuffStats = stats
                .Where(s => _definitionExtension.BuffStats.ContainsKey(s.StatId))
                .ToList();
            var remainingStats = stats.Except(untranslatedBuffStats).ToList();
            var buffStats = untranslatedBuffStats
                .Select(s => new BuffStat(s, _definitionExtension.BuffStats[s.StatId]))
                .ToList();
            return (remainingStats, buffStats);
        }

        private (IReadOnlyList<UntranslatedStat>, IReadOnlyList<UntranslatedStat>) SplitPassiveStats(
            IReadOnlyList<UntranslatedStat> stats)
        {
            var passiveStats = stats
                .Where(s => _definitionExtension.PassiveStats.Contains(s.StatId))
                .ToList();
            var remainingStats = stats.Except(passiveStats).ToList();
            return (remainingStats, passiveStats);
        }

        private (IReadOnlyList<UntranslatedStat>, IReadOnlyList<IReadOnlyList<UntranslatedStat>>) SplitStatsIntoParts(
            IReadOnlyList<UntranslatedStat> stats)
        {
            var statsPerPart = _definitionExtension.PartExtensions.Select(e => e.ModifyStats(stats)).ToList();
            var commonStats = statsPerPart.Aggregate((l, r) => l.Intersect(r)).ToList();
            var additionalStatsPerPart = statsPerPart.Select(s => s.Except(commonStats).ToList()).ToList();
            return (commonStats, additionalStatsPerPart);
        }

        private static SkillTooltipDefinition DeserializeTooltip(JToken levelJson, JToken staticJson)
        {
            var description = levelJson["description"]?.Values<string>() ?? staticJson["description"].Values<string>();
            return new SkillTooltipDefinition(
                Value<string>("name"),
                Stats("properties"),
                Stats("requirements"),
                description.ToList(),
                Stats("quality_stats"),
                Stats("stats"));

            T Value<T>(string propertyName) where T : class
                => levelJson[propertyName]?.Value<T>() ?? staticJson.Value<T>(propertyName);

            IReadOnlyList<TranslatedStat> Stats(string propertyName)
                => DeserializeStats(
                    Value<JArray>(propertyName), staticJson.Value<JArray>(propertyName), DeserializeTranslatedStat);
        }

        private static TranslatedStat DeserializeTranslatedStat(JToken statToken, JToken staticStatToken)
        {
            if (statToken.Type == JTokenType.String)
                return new TranslatedStat(statToken.Value<string>());

            var text = statToken["text"]?.Value<string>() ?? staticStatToken.Value<string>("text");
            return TryGetValues("values", statToken, staticStatToken, out double[] values)
                ? new TranslatedStat(text, values)
                : new TranslatedStat(text, GetValue<double>("value", statToken, staticStatToken));
        }

        private static IReadOnlyList<T> DeserializeStats<T>(
            JArray statsJson, JArray staticStatsJson, Func<JToken, JToken, T> deserializeStat)
        {
            if (statsJson is null)
                throw new ArgumentNullException(nameof(statsJson));
            if (staticStatsJson is null)
                staticStatsJson = statsJson;

            var stats = new List<T>(statsJson.Count);
            for (var i = 0; i < statsJson.Count; i++)
            {
                var statToken = statsJson[i];
                var staticStatToken = staticStatsJson[i];
                if (statToken.Type == JTokenType.Null)
                    statToken = staticStatToken;
                stats.Add(deserializeStat(statToken, staticStatToken));
            }
            return stats;
        }

        private static bool TryGetValues<T>(string propertyName, JToken firstToken, JToken secondToken, out T[] values)
        {
            var firstArray = GetValue<JArray>(propertyName, firstToken, secondToken);
            if (firstArray is null)
            {
                values = null;
                return false;
            }

            var secondArray = GetValue<JArray>(propertyName, secondToken, firstToken);
            if (firstArray.Count != secondArray.Count)
                throw new InvalidOperationException("Both arrays must be of the same size");

            values = new T[firstArray.Count];
            for (var i = 0; i < firstArray.Count; i++)
            {
                var first = firstArray[i];
                values[i] = first.Type == JTokenType.Null ? secondArray[i].Value<T>() : first.Value<T>();
            }
            return true;
        }

        private static T GetValue<T>(string propertyName, JToken firstToken, JToken secondToken)
        {
            if (firstToken is JObject firstObject && firstObject.TryGetValue(propertyName, out var outToken))
                return outToken.Value<T>();
            if (secondToken is JObject secondObject && secondObject.TryGetValue(propertyName, out outToken))
                return outToken.Value<T>();
            return default;
        }
    }
}