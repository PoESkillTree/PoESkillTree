using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.GameModel.Modifiers
{
    public static class ModifierJsonDeserializer
    {
        public static ModifierDefinitions Deserialize(JObject modJson)
        {
            var definitions = modJson.Properties()
                .Select(Deserialize)
                .Where(d => d.Domain != ModDomain.Area && d.Domain != ModDomain.Atlas)
                .ToList();
            return new ModifierDefinitions(definitions);
        }

        private static ModifierDefinition Deserialize(JProperty modProperty)
        {
            return new ModifierDefinition(
                modProperty.Name,
                Value<ModDomain>("domain"),
                Value<ModGenerationType>("generation_type"),
                Value<IReadOnlyList<ModifierSpawnWeight>>("spawn_weights"),
                DeserializeStats(modProperty.Value.Value<JArray>("stats")));

            T Value<T>(string key)
                => modProperty.Value[key].ToObject<T>();
        }

        private static IReadOnlyList<CraftableStat> DeserializeStats(JArray array)
            => array.Select(DeserializeStat).ToList();

        private static CraftableStat DeserializeStat(JToken token)
            => new CraftableStat(
                token.Value<string>("id"),
                token.Value<int>("min"),
                token.Value<int>("max"));
    }
}