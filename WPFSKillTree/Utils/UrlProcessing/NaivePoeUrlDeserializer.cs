using System;
using System.Text.RegularExpressions;
using POESKillTree.Localization;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from urls.<para/>
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
            var bytes = GetRawData();

            var deserializedData = new BuildUrlData(BanditConverter.Default);
            deserializedData.Version = bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];

            if (!IsVersionSupported(deserializedData.Version))
            {
                throw new NotSupportedException(
                    L10n.Message("The build you are trying to load is using an old version of the passive tree and will not work."));
            }

            deserializedData.CharacterClassId = bytes[4];
            deserializedData.AscendancyClassId = bytes[5];

            for (int k = (deserializedData.Version > 3 ? 7 : 6); k < bytes.Length; k += 2)
            {
                ushort nodeId = (ushort)(bytes[k] << 8 | bytes[k + 1]);
                deserializedData.SkilledNodesIds.Add(nodeId);
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
        /// Validates provided <paramref name="version"/><para/>.
        /// </summary>
        /// <param name="version">The verion to validate.</param>
        /// <returns>true, if provided version is supported; otherwise false.</returns>
        protected virtual bool IsVersionSupported(int version)
        {
            return true;
        }

        /// <summary>
        /// Converts build data from the specified url to an array of bytes.
        /// </summary>
        protected virtual byte[] GetRawData()
        {
            if (_rawData != null)
                return _rawData;

            var match = _urlRegex.Match(BuildUrl);

            if (!match.Success)
                throw new Exception(L10n.Message("Failed to load build from URL."));

            var newUrl = match.Groups["build"].Value.Replace("-", "+").Replace("_", "/");
            _rawData = Convert.FromBase64String(newUrl);

            return _rawData;
        }
    }
}