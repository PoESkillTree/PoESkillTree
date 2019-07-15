using System.Collections.Generic;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents build information, stored in a url. Essentially it provides just identifiers.
    /// </summary>
    /// <remarks>Currently it also can contain unused jewels information from http://poeplanner.com.</remarks>
    public class BuildUrlData
    {
        public int Version { get; set; }
        public CharacterClass CharacterClass { get; set; }
        public int AscendancyClassId { get; set; }
        public Bandit? Bandit { get; set; }
        public List<ushort> SkilledNodesIds { get; } = new List<ushort>();
        public Dictionary<int, List<byte>> Jewels { get; } = new Dictionary<int, List<byte>>();
        public List<string> CompatibilityIssues { get; } = new List<string>();
    }
}