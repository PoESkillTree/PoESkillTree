using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumsNET;
using MoreLinq;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillJsonDeserializer
    {
        private static readonly IReadOnlyList<string> BuffProvidingActiveSkillTypes = new[]
        {
            ActiveSkillType.Buff, ActiveSkillType.ExplicitProvidesBuff, ActiveSkillType.Curse
        };

        private int _nextNumericId;

        public static async Task<SkillDefinitions> DeserializeAsync()
        {
            var gemJsonTask = DataUtils.LoadRePoEAsync("gems");
            var gemTooltipJsonTask = DataUtils.LoadRePoEAsync("gem_tooltips");
            var gemJson = JObject.Parse(await gemJsonTask.ConfigureAwait(false));
            MergeStaticWithPerLevel(gemJson);
            var gemTooltipJson = JObject.Parse(await gemTooltipJsonTask.ConfigureAwait(false));
            MergeStaticWithPerLevel(gemTooltipJson);
            return DeserializeMerged(gemJson, gemTooltipJson);
        }

        public static SkillDefinitions Deserialize(JObject gemJson, JObject gemTooltipJson)
        {
            MergeStaticWithPerLevel(gemJson);
            MergeStaticWithPerLevel(gemTooltipJson);
            return DeserializeMerged(gemJson, gemTooltipJson);
        }

        private static SkillDefinitions DeserializeMerged(JObject gemJson, JObject gemTooltipJson)
        {
            var deserializer = new SkillJsonDeserializer();
            var definitions = gemJson.Properties()
                .Select(property => deserializer.Deserialize(property, gemTooltipJson))
                .Where(d => d != null);
            return new SkillDefinitions(definitions.ToList());
        }

        private SkillDefinition Deserialize(JProperty gemProperty, JToken gemTooltipJson)
            => Deserialize(gemProperty.Name, (JObject) gemProperty.Value,
                gemTooltipJson.Value<JObject>(gemProperty.Name));

        private SkillDefinition Deserialize(string skillId, JObject gemJson, JObject gemTooltipJson)
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
            var levels = DeserializeLevels(gemJson, gemTooltipJson);

            if (gemJson.TryGetValue("active_skill", out var activeSkillJson))
            {
                var displayName = activeSkillJson.Value<string>("display_name");
                var activeSkillTypes = activeSkillJson["types"].Values<string>().ToHashSet();
                var minionActiveSkillTypes = activeSkillJson["minion_types"]?.Values<string>().ToHashSet()
                                             ?? new HashSet<string>();
                var keywords = GetKeywords(displayName, activeSkillTypes, gemTags);
                var providesBuff = activeSkillTypes.ContainsAny(BuffProvidingActiveSkillTypes)
                                   || minionActiveSkillTypes.ContainsAny(BuffProvidingActiveSkillTypes);
                var totemLifeMultiplier = activeSkillJson.Value<double?>("skill_totem_life_multiplier");
                var weaponRestrictions = activeSkillJson["weapon_restrictions"].Values<string>()
                    .Select(ItemClassEx.Parse).ToList();
                var activeSkillDefinition = new ActiveSkillDefinition(
                    displayName, castTime, activeSkillTypes, minionActiveSkillTypes, keywords, providesBuff,
                    totemLifeMultiplier, weaponRestrictions);
                return SkillDefinition.CreateActive(skillId, numericId, statTranslationFile,
                    baseItemDefinition, activeSkillDefinition, levels);
            }
            else
            {
                var supportSkillJson = gemJson["support_gem"];
                var supportSkillDefinition = new SupportSkillDefinition(
                    supportSkillJson.Value<bool>("supports_gems_only"),
                    supportSkillJson["allowed_types"].Values<string>().ToList(),
                    supportSkillJson["excluded_types"].Values<string>().ToList(),
                    supportSkillJson["added_types"].Values<string>().ToList());
                return SkillDefinition.CreateSupport(skillId, numericId, statTranslationFile,
                    baseItemDefinition, supportSkillDefinition, levels);
            }
        }

        private static IReadOnlyList<Keyword> GetKeywords(
            string displayName, ISet<string> activeSkillTypes, ISet<string> gemTags)
        {
            return Enums.GetValues<Keyword>()
                .Where(k => k.IsOnSkill(displayName, activeSkillTypes, gemTags)).ToList();
        }

        private static IReadOnlyDictionary<int, SkillLevelDefinition> DeserializeLevels(
            JObject gemJson, JObject gemTooltipJson)
        {
            var perLevel = gemJson["per_level"];
            var levels = new Dictionary<int, SkillLevelDefinition>();
            foreach (var perLevelProp in perLevel.Cast<JProperty>())
            {
                levels[int.Parse(perLevelProp.Name)] =
                    DeserializeLevel((JObject) perLevelProp.Value, gemTooltipJson["per_level"][perLevelProp.Name]);
            }
            return levels;
        }

        private static SkillLevelDefinition DeserializeLevel(JObject levelJson, JToken tooltipLevelJson)
        {
            var statRequirements = levelJson["stat_requirements"];
            return new SkillLevelDefinition(
                Value<int?>("damage_effectiveness") / 100D + 1,
                Value<int?>("damage_multiplier") / 10000D + 1,
                Value<int?>("crit_chance") / 100D,
                Value<int?>("mana_cost"),
                Value<int?>("mana_multiplier") / 100D,
                Value<int?>("mana_reservation_override"),
                Value<int?>("cooldown"),
                Value<int>("required_level"),
                statRequirements?.Value<int>("dex") ?? 0,
                statRequirements?.Value<int>("int") ?? 0,
                statRequirements?.Value<int>("str") ?? 0,
                Stats("quality_stats"),
                Stats("stats"),
                DeserializeTooltip(tooltipLevelJson));

            T Value<T>(string propertyName) => levelJson.Value<T>(propertyName);

            IReadOnlyList<UntranslatedStat> Stats(string propertyName)
                => Value<JArray>(propertyName)
                    .Select(s => new UntranslatedStat(s.Value<string>("id"), s.Value<int>("value")))
                    .ToList();
        }

        private static SkillTooltipDefinition DeserializeTooltip(JToken tooltipJson)
            => new SkillTooltipDefinition(
                tooltipJson.Value<string>("name"),
                DeserializeTranslatedStatArray(tooltipJson.Value<JArray>("properties")),
                DeserializeTranslatedStatArray(tooltipJson.Value<JArray>("requirements")),
                tooltipJson["description"].Values<string>().ToList(),
                DeserializeTranslatedStatArray(tooltipJson.Value<JArray>("quality_stats")),
                DeserializeTranslatedStatArray(tooltipJson.Value<JArray>("stats")));

        private static IReadOnlyList<TranslatedStat> DeserializeTranslatedStatArray(JArray statJson)
        {
            var stats = new List<TranslatedStat>();
            foreach (var token in statJson)
            {
                TranslatedStat stat;
                if (token.Type == JTokenType.String)
                {
                    stat = new TranslatedStat(token.Value<string>());
                }
                else
                {
                    var text = token.Value<string>("text");
                    stat = ((JObject) token).TryGetValue("values", out var values)
                        ? new TranslatedStat(text, values.Values<double>().ToArray())
                        : new TranslatedStat(text, token.Value<double>("value"));
                }
                stats.Add(stat);
            }
            return stats;
        }

        private static void MergeStaticWithPerLevel(JObject json)
        {
            foreach (var token in json.PropertyValues())
            {
                var staticObject = token.Value<JObject>("static");
                foreach (var perLevelProp in token["per_level"].Cast<JProperty>())
                {
                    MergeObject(staticObject, (JObject) perLevelProp.Value);
                }
            }
        }

        private static void MergeObject(JObject staticObject, JObject perLevelObject)
        {
            foreach (var staticProp in staticObject.Properties())
            {
                var staticValue = staticProp.Value;
                var staticType = staticValue.Type;
                var propName = staticProp.Name;
                if (staticType == JTokenType.Null)
                {
                    continue;
                }
                if (perLevelObject.TryGetValue(propName, out var perLevelValue))
                {
                    if (staticType == JTokenType.Object)
                    {
                        MergeObject((JObject) staticValue, (JObject) perLevelValue);
                    }
                    else if (staticType == JTokenType.Array)
                    {
                        MergeArray((JArray) staticValue, (JArray) perLevelValue);
                    }
                    // keep the one from perLevelObject for primitives
                }
                else
                {
                    perLevelObject[propName] = staticValue;
                }
            }
        }

        private static void MergeArray(JArray staticArray, JArray perLevelArray)
        {
            if (staticArray.Count != perLevelArray.Count)
                throw new ArgumentException("both JArrays must be of the same size");
            for (int i = 0; i < staticArray.Count; i++)
            {
                var staticValue = staticArray[i];
                var perLevelValue = perLevelArray[i];
                if (staticValue.Type == JTokenType.Null)
                {
                    continue;
                }
                switch (perLevelValue.Type)
                {
                    case JTokenType.Null:
                        perLevelArray[i] = staticValue;
                        break;
                    case JTokenType.Object:
                        MergeObject((JObject) staticValue, (JObject) perLevelValue);
                        break;
                    case JTokenType.Array:
                        MergeArray((JArray) staticValue, (JArray) perLevelValue);
                        break;
                    // keep the one from perLevelObject for primitives
                }
            }
        }
    }
}