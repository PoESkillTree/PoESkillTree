using Newtonsoft.Json;
using PoESkillTree.Engine.GameModel.Modifiers;

namespace PoESkillTree.Model.Items.Mods
{
    // The classes used to deserialize RePoE's mods

    public class JsonMod
    {
        // string[] "adds_tags": not used
        [JsonProperty("domain")]
        public ModDomain Domain { get; set; }
        [JsonProperty("generation_type")]
        public ModGenerationType GenerationType { get; set; }
        // "generation_weights": not used
        // "grants_buff": not used
        // "grants_effect": not used
        [JsonProperty("group")]
        public string Group { get; set; } = default!;
        [JsonProperty("is_essence_only")]
        public bool IsEssenceOnly { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = default!;
        [JsonProperty("required_level")]
        public int RequiredLevel { get; set; }
        [JsonProperty("spawn_weights")]
        public JsonSpawnWeight[] SpawnWeights { get; set; } = default!;
        [JsonProperty("stats")]
        public JsonStat[] Stats { get; set; } = default!;
    }

    public class JsonSpawnWeight
    {
        [JsonProperty("tag")]
        public string Tag { get; set; } = default!;
        [JsonProperty("weight")]
        public int Weight { get; set; }
        [JsonIgnore]
        public bool CanSpawn => Weight > 0;
    }

    public class JsonStat
    {
        [JsonProperty("id")]
        public string Id { get; set; } = default!;
        [JsonProperty("max")]
        public int Max { get; set; }
        [JsonProperty("min")]
        public int Min { get; set; }
    }
}