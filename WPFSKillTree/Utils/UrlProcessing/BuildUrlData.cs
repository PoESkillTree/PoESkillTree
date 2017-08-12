using System.Collections.Generic;
using POESKillTree.Model;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents build information, stored in a url. Essentially it provides just identifiers.
    /// </summary>
    /// <remarks>Currently it also can contain unused jewels information from http://poeplanner.com.</remarks>
    public class BuildUrlData
    {
        private readonly BanditConverter _converter;
        private int? _banditId;

        public int Version { get; set; }
        public int CharacterClassId { get; set; }
        public int AscendancyClassId { get; set; }
        public List<ushort> SkilledNodesIds { get; } = new List<ushort>();
        public Dictionary<int, List<byte>> Jewels { get; } = new Dictionary<int, List<byte>>();
        public List<string> CompatibilityIssues { get; } = new List<string>();

        public virtual Bandit Bandit => _converter.GetBandit(_banditId);

        public BuildUrlData(BanditConverter converter)
        {
            _converter = converter;
        }

        public void SetBandit(int id)
        {
            _banditId = id;
        }

        public bool HasAnyBanditValue()
        {
            return _banditId.HasValue;
        }

        public bool BanditsAreSame(BanditSettings bandits)
        {
            return Bandit == bandits.Choice;
        }
    }
}