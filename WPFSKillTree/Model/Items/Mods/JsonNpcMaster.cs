using Newtonsoft.Json;

namespace POESKillTree.Model.Items.Mods
{
    // The classes used to deserialize RePoE's npc_master

    public class JsonNpcMaster
    {
        [JsonProperty("signature_mod")]
        public JsonSignatureMod SignatureMod { get; set; }
    }

    public class JsonSignatureMod
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("spawn_weights")]
        public JsonSpawnWeight[] SpawnWeights { get; set; }
    }
}