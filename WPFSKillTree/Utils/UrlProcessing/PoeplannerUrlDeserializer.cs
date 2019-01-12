using System;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.GameModel;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from the http://poeplanner.com urls.
    /// </summary>
    public class PoeplannerUrlDeserializer : BuildUrlDeserializer
    {
        private static readonly Regex UrlRegex = new Regex(@"(http(|s):\/\/|)(\w*\.|)poeplanner\.com\/(?<build>[\w-=]+)");
        private byte[] _rawData;

        /// <summary>
        /// Initializes a new instance of the <see cref="PoeplannerUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The poeplanner build url.</param>
        /// <param name="ascendancyClasses">The instance of the <see cref="ascendancyClasses"/>
        /// to access general information about skill tree.</param>
        private PoeplannerUrlDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses) : base(buildUrl, ascendancyClasses)
        {
        }

        /// <summary>
        /// Creates the <see cref="PoeplannerUrlDeserializer"/> class instance if specified url is valid.
        /// </summary>
        /// <param name="buildUrl">A string containing a build url.</param>
        /// <param name="deserializer">When this method returns, contains the deserializer instance or null, if url conversion is impossible.</param>
        /// <returns>true if deserializer was created successfully; otherwise, false.</returns>
        public static bool TryCreate(string buildUrl, IAscendancyClasses ascendancyClasses, out BuildUrlDeserializer deserializer)
        {
            if (!UrlRegex.IsMatch(buildUrl))
            {
                deserializer = null;
                return false;
            }

            deserializer = new PoeplannerUrlDeserializer(buildUrl, ascendancyClasses);
            return true;
        }

        public override bool ValidateBuildUrl(out Exception exception)
        {
            try
            {
                GetRawData();
                exception = null;
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        public override BuildUrlData GetBuildData()
        {
            PoeplannerData data = DecodePoeplannerUrl();

            BuildUrlData buildData = ParsePoeplannerData(data);

            return buildData;
        }

        protected override int GetCharacterClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 6 ? 0 : rawData[5] & 15;
        }

        protected override int GetAscendancyClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 6 ? 0 : rawData[5] >> 4 & 15;
        }

        #region Helpers

        private byte[] GetRawData()
        {
            if (_rawData != null)
                return _rawData;

            var buildSegment = BuildUrl.Split('/').LastOrDefault();

            if (buildSegment == null)
                return new byte[0];

            buildSegment = buildSegment
                .Replace("-", "+")
                .Replace("_", "/");

            _rawData = Convert.FromBase64String(buildSegment);

            return _rawData;
        }

        private PoeplannerData DecodePoeplannerUrl()
        {
            byte[] rawBytes = GetRawData();

            var skillsBuffSize = rawBytes[3] << 8 | rawBytes[4];
            var aurasBuffSize = rawBytes[5 + skillsBuffSize] << 8 | rawBytes[6 + skillsBuffSize];
            var equipBuffSize = rawBytes[7 + skillsBuffSize + aurasBuffSize] << 8 | rawBytes[8 + skillsBuffSize + aurasBuffSize];

            var data = new PoeplannerData
            {
                Version = rawBytes[0] << 8 | rawBytes[1],
                ActiveTab = rawBytes[2],
                NodesData = new byte[skillsBuffSize],
                AurasData = new byte[aurasBuffSize],
                EquipmentData = new byte[equipBuffSize]
            };

            Array.Copy(rawBytes, 5, data.NodesData, 0, skillsBuffSize);
            Array.Copy(rawBytes, 7 + skillsBuffSize, data.AurasData, 0, aurasBuffSize);
            Array.Copy(rawBytes, 9 + skillsBuffSize + aurasBuffSize, data.EquipmentData, 0, equipBuffSize);

            return data;
        }

        private BuildUrlData ParsePoeplannerData(PoeplannerData data)
        {
            var result = new BuildUrlData(BanditConverter.PoEPlanner);

            result.Version = data.Version;

            // There is a small bug in poeplanner, where class and ascendancy bytes are missing, when no one node was selected.
            // Need to check length
            if (data.NodesData.Length == 0)
                return result;

            result.CharacterClass = (CharacterClass) (data.NodesData[0] & 15);
            result.AscendancyClassId = data.NodesData[0] >> 4 & 15;

            if (data.NodesData.Length < 2)
                return result;

            result.SetBandit(data.NodesData[1] & 3);

            if (data.NodesData.Length < 4)
                return result;

            var skilledNodesCount = data.NodesData[2] << 8 | data.NodesData[3];
            int i = 4;
            while (i < 2 * skilledNodesCount + 4)
            {
                result.SkilledNodesIds.Add((ushort)(data.NodesData[i++] << 8 | data.NodesData[i++]));
            }

            var jeweledNodesCount = data.NodesData[i++];
            for (var j = 0; j < jeweledNodesCount; j++)
            {
                var nodeId = data.NodesData[i++] << 8 | data.NodesData[i++];
                var jewelsDataBuffSize = data.NodesData[i++];

                var rawJewelData = data.NodesData.Skip(i++).Take(jewelsDataBuffSize).ToList();
                i = i + (jewelsDataBuffSize - 1);
                result.Jewels[nodeId] = rawJewelData;
            }

            return result;
        }

        /// <summary>
        /// Represents preprocessed raw data.
        /// </summary>
        protected class PoeplannerData
        {
            public int Version { get; set; }
            public byte ActiveTab { get; set; }
            public byte[] NodesData { get; set; }
            internal byte[] AurasData { get; set; }
            internal byte[] EquipmentData { get; set; }
        }

        #endregion
    }
}