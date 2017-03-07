using System.Text.RegularExpressions;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from the https://pathofexile.com urls.
    /// </summary>
    public class PathofexileUrlDeserializer : NaivePoEUrlDeserializer
    {
        private int _supportedVersion = 4;
        private static readonly Regex UrlRegex = new Regex(@"(http(|s):\/\/|)(\w*\.|)pathofexile\.com\/(fullscreen-|)passive-skill-tree\/(?<build>[\w-=]+)");

        /// <summary>
        /// Initializes a new instance of the <see cref="PathofexileUrlDeserializer"/> class disregarding the specified url format.
        /// </summary>
        /// <param name="buildUrl">The https://pathofexile.com build url.</param>
        /// <param name="ascendancyClasses">The instance of the <see cref="ascendancyClasses"/>
        /// to access general information about skill tree.</param>
        private PathofexileUrlDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses) : base(buildUrl, ascendancyClasses)
        {
        }

        /// <summary>
        /// Creates the <see cref="PathofexileUrlDeserializer"/> class instance if specified url is valid.
        /// </summary>
        /// <param name="buildUrl">The string containing a build url.</param>
        /// <param name="deserializer">When this method returns, contains the deserializer instance or null, if url conversion is impossible.</param>
        /// <returns>true if deserializer was created successfully; otherwise, false.</returns>
        public static bool TryCreate(string buildUrl, IAscendancyClasses ascendancyClasses, out BuildUrlDeserializer deserializer)
        {
            if (!UrlRegex.IsMatch(buildUrl))
            {
                deserializer = null;
                return false;
            }

            deserializer = new PathofexileUrlDeserializer(buildUrl, ascendancyClasses);
            return true;
        }

        protected override bool IsVersionCompatible(int version)
        {
            return version >= _supportedVersion;
        }
    }
}