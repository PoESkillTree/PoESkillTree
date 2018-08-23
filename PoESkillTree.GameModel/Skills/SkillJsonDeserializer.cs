using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillJsonDeserializer
    {
        private int _nextNumericId;

        public static SkillDefinitions Deserialize(JObject gemJson, JObject gemTooltipJson)
        {
            var deserializer = new SkillJsonDeserializer();
            var definitions = gemJson.Properties()
                .Select(property => deserializer.Deserialize(property.Name, (JObject) property.Value));
            return new SkillDefinitions(definitions.ToList());
        }

        private SkillDefinition Deserialize(string skillId, JObject gemJson)
        {
            var gemTags = gemJson["tags"].Values<string>();
            var activeSkillJson = gemJson["active_skill"];
            var displayName = activeSkillJson.Value<string>("display_name");
            var activeSkillTypes = activeSkillJson["types"].Values<string>();
            var activeSkillDefinition = new ActiveSkillDefinition(
                displayName, GetKeywords(displayName, activeSkillTypes, gemTags), false);
            return SkillDefinition.CreateActive(skillId, _nextNumericId++, activeSkillDefinition);
        }

        private IReadOnlyList<Keyword> GetKeywords(
            string displayName, IEnumerable<string> activeSkillTypes, IEnumerable<string> gemTags)
        {
            var typesSet = new HashSet<string>(activeSkillTypes);
            var tagsSet = new HashSet<string>(gemTags);
            return Enums.GetValues<Keyword>()
                .Where(k => k.IsOnSkill(displayName, typesSet, tagsSet)).ToList();
        }
    }
}