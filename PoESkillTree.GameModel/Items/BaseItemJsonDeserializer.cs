using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumsNET;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Modifiers;

namespace PoESkillTree.GameModel.Items
{
    /// <summary>
    /// Deserializes <see cref="BaseItemDefinitions"/> from RePoE's base_items and mods files.
    /// </summary>
    public class BaseItemJsonDeserializer
    {
        private readonly ModifierDefinitions _modifiers;

        private BaseItemJsonDeserializer(ModifierDefinitions modifiers) => _modifiers = modifiers;

        public static async Task<BaseItemDefinitions> DeserializeAsync(
            bool deserializeOnThreadPool, Task<ModifierDefinitions> modifiersTask)
        {
            var baseItemsTask = DataUtils.LoadRePoEAsObjectAsync("base_items", deserializeOnThreadPool);
            return Deserialize(
                await baseItemsTask.ConfigureAwait(false),
                await modifiersTask.ConfigureAwait(false));
        }

        public static BaseItemDefinitions Deserialize(JObject baseItemJson, ModifierDefinitions modifiers)
        {
            var deserializer = new BaseItemJsonDeserializer(modifiers);
            var definitions = baseItemJson.Properties()
                .Select(deserializer.Deserialize)
                .Where(d => d.ReleaseState != ReleaseState.Unreleased)
                .ToList();
            return new BaseItemDefinitions(definitions);
        }

        private BaseItemDefinition Deserialize(JProperty itemProperty)
        {
            var id = itemProperty.Name;
            var json = itemProperty.Value;

            var rawTags = Values<string>("tags").ToList();
            return new BaseItemDefinition(id,
                Value<string>("name"),
                ItemClassEx.Parse(Value<string>("item_class")),
                rawTags,
                DeserializeTags(rawTags),
                DeserializeProperties(Value<JObject>("properties")),
                DeserializeGrantsBuff(json["grants_buff"]),
                DeserializeRequirements(json["requirements"]),
                DeserializeImplicitModifiers(Values<string>("implicits")),
                Value<int>("inventory_height"),
                Value<int>("inventory_width"),
                Value<int>("drop_level"),
                Enums.Parse<ReleaseState>(Value<string>("release_state"), EnumFormat.EnumMemberValue),
                json["visual_identity"].Value<string>("dds_file"));

            T Value<T>(string propertyName) => json.Value<T>(propertyName);

            IEnumerable<T> Values<T>(string propertyName) => json[propertyName].Values<T>();
        }

        private static Tags DeserializeTags(IEnumerable<string> rawTags)
        {
            var tags = Tags.Default;
            foreach (var rawTag in rawTags)
            {
                if (TagsExtensions.TryParse(rawTag, out var tag))
                {
                    tags |= tag;
                }
            }
            return tags;
        }

        private static IReadOnlyList<Property> DeserializeProperties(JObject propertiesJson)
            => propertiesJson.Properties()
                .Where(p => p.Value.Type == JTokenType.Integer)
                .Select(DeserializeProperty).ToList();

        private static Property DeserializeProperty(JProperty jsonProperty)
            => new Property(jsonProperty.Name, jsonProperty.Value.Value<int>());

        private static IReadOnlyList<UntranslatedStat> DeserializeGrantsBuff(JToken buffJson)
        {
            if (buffJson is null)
                return new UntranslatedStat[0];
            return buffJson.Value<JObject>("stats").Properties().Select(DeserializeUntranslatedStat).ToList();
        }

        private static UntranslatedStat DeserializeUntranslatedStat(JProperty jsonProperty)
            => new UntranslatedStat(jsonProperty.Name, jsonProperty.Value.Value<int>());

        private static Requirements DeserializeRequirements(JToken requirementsJson)
        {
            if (!requirementsJson.HasValues)
                return new Requirements(0, 0, 0, 0);
            return requirementsJson.ToObject<Requirements>();
        }

        private IReadOnlyList<CraftableStat> DeserializeImplicitModifiers(IEnumerable<string> implicits)
            => implicits.SelectMany(i => _modifiers.GetModifierById(i).Stats).ToList();
    }
}