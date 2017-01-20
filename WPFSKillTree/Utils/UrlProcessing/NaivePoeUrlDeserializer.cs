using System;
using System.Linq;
using System.Text.RegularExpressions;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from urls.
    /// This class uses nonstrict url check and consider specified url as compatible with the official planner https://pathofexile.com.
    /// </summary>
    public class NaivePoEUrlDeserializer : BuildUrlDeserializer
    {
        private readonly Regex _urlRegex = new Regex(@".*\/(?<build>[\w-=]+)");
        private byte[] _rawData;

        /// <summary>
        /// Initializes a new instance of the <see cref="NaivePoEUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        public NaivePoEUrlDeserializer(string buildUrl) : base(buildUrl)
        {
        }

        public override BuildUrlData GetBuildData()
        {
            var deserializedData = new BuildUrlData();

            var decbuff = GetRawData();
            BitConverter.ToInt32(new[] { decbuff[3], decbuff[2], decbuff[1], decbuff[0] }, 0);

            deserializedData.CharacterClassId = decbuff[4];
            deserializedData.AscendancyClassId = decbuff[5];

            int version = BitConverter.ToInt32(new[] { decbuff[3], decbuff[2], decbuff[1], decbuff[0] }, 0);
            for (int k = (version > 3 ? 7 : 6); k < decbuff.Length; k += 2)
            {
                byte[] dbff = { decbuff[k + 1], decbuff[k + 0] };
                if (SkillTree.Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                {
                    deserializedData.SkilledNodesIds.Add(BitConverter.ToUInt16(dbff, 0));
                }
            }

            return deserializedData;
        }

        public override int GetCharacterClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 5 ? 0 : rawData[4];
        }

        public override int GetAscendancyClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 6 ? 0 : rawData[5];
        }

        /// <summary>
        /// Converts build data from the specified url to an array of bytes.
        /// </summary>
        protected virtual byte[] GetRawData()
        {
            if (_rawData != null)
                return _rawData;

            var newUrl = Regex.Replace(BuildUrl, @"\s", "");
            var match = _urlRegex.Match(newUrl);

            if (!match.Success)
                throw new Exception(L10n.Message($"Unable to deserialize specified Url: {BuildUrl}."));

            newUrl = match.Groups["build"].Value.Replace("-", "+").Replace("_", "/");
            _rawData = Convert.FromBase64String(newUrl);

            return _rawData;
        }
    }
}