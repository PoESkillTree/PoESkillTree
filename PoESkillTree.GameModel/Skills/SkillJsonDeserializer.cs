using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillJsonDeserializer
    {
        private int _nextNumericId;

        public static SkillDefinitions Deserialize(JObject gemJson, JObject gemTooltipJson)
        {
            MergeStaticWithPerLevel(gemJson);
            MergeStaticWithPerLevel(gemTooltipJson);
            var deserializer = new SkillJsonDeserializer();
            var definitions = gemJson.Properties()
                .Select(property => deserializer.Deserialize(property, gemTooltipJson));
            return new SkillDefinitions(definitions.ToList());
        }

        private SkillDefinition Deserialize(JProperty gemProperty, JToken gemTooltipJson)
            => Deserialize(gemProperty.Name, (JObject) gemProperty.Value,
                gemTooltipJson.Value<JObject>(gemProperty.Name));

        private SkillDefinition Deserialize(string skillId, JObject gemJson, JObject gemTooltipJson)
        {
            var gemTags = gemJson["tags"].Values<string>();
            var activeSkillJson = gemJson["active_skill"];
            var displayName = activeSkillJson.Value<string>("display_name");
            var activeSkillTypes = activeSkillJson["types"].Values<string>().ToHashSet();
            var activeSkillDefinition = new ActiveSkillDefinition(
                displayName, activeSkillTypes, GetKeywords(displayName, activeSkillTypes, gemTags), false);
            var levels = DeserializeLevels(gemJson, gemTooltipJson);
            return SkillDefinition.CreateActive(skillId, _nextNumericId++, activeSkillDefinition, levels);
        }

        private static IReadOnlyList<Keyword> GetKeywords(
            string displayName, ISet<string> activeSkillTypes, IEnumerable<string> gemTags)
        {
            var tagsSet = new HashSet<string>(gemTags);
            return Enums.GetValues<Keyword>()
                .Where(k => k.IsOnSkill(displayName, activeSkillTypes, tagsSet)).ToList();
        }

        private static IReadOnlyDictionary<int, SkillLevelDefinition> DeserializeLevels(
            JObject gemJson, JObject gemTooltipJson)
        {
            var perLevel = gemJson["per_level"];
            var levels = new Dictionary<int, SkillLevelDefinition>();
            foreach (var perLevelProp in perLevel.Cast<JProperty>())
            {
                levels[int.Parse(perLevelProp.Name)] =
                    DeserializeLevel(perLevelProp.Value, gemTooltipJson["per_level"][perLevelProp.Name]);
            }
            return levels;
        }

        private static SkillLevelDefinition DeserializeLevel(JToken levelJson, JToken tooltipLevelJson)
        {
            var statRequirements = levelJson["stat_requirements"];
            return new SkillLevelDefinition(
                Value<int>("damage_effectiveness") / 100D + 1,
                Value<int>("damage_multiplier") / 10000D + 1,
                Value<int>("mana_cost"),
                Value<int>("required_level"),
                statRequirements.Value<int>("dex"),
                statRequirements.Value<int>("int"),
                statRequirements.Value<int>("str"),
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