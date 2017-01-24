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
        private int? _banditNormalId;
        private int? _banditCruelId;
        private int? _banditMercilessId;

        public BuildUrlData(BanditConverter converter)
        {
            _converter = converter;
        }

        public int Version { get; set; }
        public int CharacterClassId { get; set; }
        public int AscendancyClassId { get; set; }
        public List<ushort> SkilledNodesIds { get; } = new List<ushort>();
        public Dictionary<int, List<byte>> Jewels { get; } = new Dictionary<int, List<byte>>();

        public virtual Bandit BanditNormal => _converter.GetBandit(_banditNormalId);
        public virtual Bandit BanditCruel => _converter.GetBandit(_banditCruelId);
        public virtual Bandit BanditMerciless => _converter.GetBandit(_banditMercilessId);

        public void SetBanditNormal(int id)
        {
            _banditNormalId = id;
        }

        public void SetBanditCruel(int id)
        {
            _banditCruelId = id;
        }

        public void SetBanditMerciless(int id)
        {
            _banditMercilessId = id;
        }

        public bool HasAnyBanditValue()
        {
            return _banditNormalId.HasValue || _banditCruelId.HasValue || _banditMercilessId.HasValue;
        }

        public bool BanditsAreSame(BanditSettings bandits)
        {
            return BanditNormal == bandits.Normal
                && BanditCruel == bandits.Cruel
                && BanditMerciless == bandits.Merciless;
        }
    }
}