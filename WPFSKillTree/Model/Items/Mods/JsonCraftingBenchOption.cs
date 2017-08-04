using Newtonsoft.Json;

namespace POESKillTree.Model.Items.Mods
{
    // The class used to deserialize RePoE's crafting_bench_options
    public class JsonCraftingBenchOption
    {
        [JsonProperty("mod_id")]
        public string ModId { get; set; }

        [JsonProperty("item_classes")]
        public string[] ItemClasses { get; set; }
    }
}
