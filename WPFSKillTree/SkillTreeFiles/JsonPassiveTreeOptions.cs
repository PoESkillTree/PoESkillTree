using Newtonsoft.Json;
using PoESkillTree.Engine.GameModel.PassiveTree.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoESkillTree.SkillTreeFiles
{
    public class JsonPassiveTreeOptions
    {
        [JsonProperty("ascClasses")]
        public Dictionary<int, CharacterToAscendancyOption> CharacterToAscendancy { get; set; } = default!;
    }

    public class CharacterToAscendancyOption
    {
        [JsonProperty("name")]
        public string CharacterName { get; set; } = default!;

        [JsonProperty("classes")]
        public Dictionary<int, JsonPassiveTreeAscendancyClass> AscendancyClasses { get; set; } = default!;
    }
}
