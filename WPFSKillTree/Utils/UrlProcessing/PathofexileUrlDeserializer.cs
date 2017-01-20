using System.Text.RegularExpressions;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Represents an object that extracts build information from the https://pathofexile.com urls.
    /// </summary>
    public class PathofexileUrlDeserializer : NaivePoEUrlDeserializer
    {
        private static readonly Regex UrlRegex = new Regex(@"(http(|s):\/\/|)(\w*\.|)pathofexile\.com\/(fullscreen-|)passive-skill-tree\/(?<build>[\w-=]+)");

        /// <summary>
        /// Initializes a new instance of the <see cref="PathofexileUrlDeserializer"/> class disregarding the specified url format.
        /// </summary>
        /// <param name="buildUrl">The https://pathofexile.com build url.</param>
        public PathofexileUrlDeserializer(string buildUrl) : base(buildUrl)
        {
        }

        /// <summary>
        /// Creates the <see cref="PathofexileUrlDeserializer"/> class instance if specified url is valid.
        /// </summary>
        /// <param name="buildUrl">The string containing a build url.</param>
        /// <param name="deserializer">When this method returns, contains the deserializer instance or null, if url conversion is impossible.</param>
        /// <returns>true if deserializer was created successfully; otherwise, false.</returns>
        public static bool TryCreate(string buildUrl, out BuildUrlDeserializer deserializer)
        {
            if (!UrlRegex.IsMatch(buildUrl))
            {
                deserializer = null;
                return false;
            }

            deserializer = new PathofexileUrlDeserializer(buildUrl);
            return true;
        }

    }
}