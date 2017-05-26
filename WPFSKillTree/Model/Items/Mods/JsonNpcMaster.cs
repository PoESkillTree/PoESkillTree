using System.Collections.Generic;
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

        [JsonProperty("spawn_tags")]
        public Dictionary<string, bool>[] SpawnTags { get; set; }
    }
}