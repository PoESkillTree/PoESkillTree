using System.Collections.Generic;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents build information, stored in a url. Essentially it provides just identifiers.
    /// </summary>
    /// <remarks>Currently it also can contain unused jewels information from http://poeplanner.com.</remarks>
    public class BuildUrlData
    {
        public int BanditNormal { get; set; }
        public int BanditCruel { get; set; }
        public int BanditMerciless { get; set; }
        public int CharacterClassId { get; set; }
        public int AscendancyClassId { get; set; }
        public List<ushort> SkilledNodesIds { get; } = new List<ushort>();
        public Dictionary<int, List<byte>> Jewels { get; } = new Dictionary<int, List<byte>>();
    }
}