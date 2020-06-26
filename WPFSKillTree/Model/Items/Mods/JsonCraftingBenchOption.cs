using System.Collections.Generic;
using Newtonsoft.Json;

namespace PoESkillTree.Model.Items.Mods
{
    // The class used to deserialize RePoE's crafting_bench_options
    public class JsonCraftingBenchOption
    {
        [JsonProperty("actions")]
        public JsonCraftingBenchOptionActions Actions { get; set; } = default!;

        [JsonProperty("item_classes")]
        public string[] ItemClasses { get; set; } = default!;
    }

    public class JsonCraftingBenchOptionActions
    {
        [JsonProperty("add_mod")]
        public string? AddMod { get; set; }
    }
}
