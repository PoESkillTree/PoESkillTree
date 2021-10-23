using PoESkillTree.Engine.GameModel;
using System.Collections.Generic;

namespace PoESkillTree.Utils.UrlProcessing
{
    public class SkillTreeUrlData
    {
        public int Version { get; set; }
        public CharacterClass CharacterClass { get; set; }
        public int AscendancyClassId { get; set; }
        public List<ushort> SkilledNodesIds { get; set; } = new List<ushort>();
        public List<ushort> ClusterJewelNodesIds { get; set; } = new List<ushort>();
        public List<(ushort Id, ushort Effect)> MasteryEffectPairs { get; set; } = new List<(ushort, ushort)>();
        
        public bool IsValid { get; set; } = false;
        public List<string> CompatibilityIssues { get; set; } = new List<string>();
    }
}
