using System;
using System.Text.RegularExpressions;
using PoESkillTree.GameModel;
using POESKillTree.Localization;
using POESKillTree.SkillTreeFiles;

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
        /// <param name="ascendancyClasses">The instance of the <see cref="ascendancyClasses"/>
        /// to access general information about skill tree.</param>
        public NaivePoEUrlDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses) : base(buildUrl, ascendancyClasses)
        {
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
            var bytes = GetRawData();

            var deserializedData = new BuildUrlData(BanditConverter.Default);
            deserializedData.Version = bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];

            if (!IsVersionCompatible(deserializedData.Version))
            {
                deserializedData.CompatibilityIssues.Add(
                    L10n.Message("The build is using an old version of the passive tree."));
            }

            deserializedData.CharacterClass = (CharacterClass) bytes[4];
            deserializedData.AscendancyClassId = bytes[5];

            for (int k = (deserializedData.Version > 3 ? 7 : 6); k < bytes.Length; k += 2)
            {
                ushort nodeId = (ushort)(bytes[k] << 8 | bytes[k + 1]);
                deserializedData.SkilledNodesIds.Add(nodeId);
            }

            return deserializedData;
        }

        protected override int GetCharacterClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 5 ? 0 : rawData[4];
        }

        protected override int GetAscendancyClassId()
        {
            var rawData = GetRawData();

            return rawData.Length < 6 ? 0 : rawData[5];
        }

        /// <summary>
        /// Validates provided <paramref name="version"/><para/>.
        /// </summary>
        /// <param name="version">The verion to validate.</param>
        /// <returns>true, if provided version is supported; otherwise false.</returns>
        protected virtual bool IsVersionCompatible(int version)
        {
            // Naive deserializer consumes everything, it cannot predict version of an unknown tree format
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