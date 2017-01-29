using System;
using System.Collections.Generic;

namespace POESKillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Creates an instances of a class derived from the <see cref="BuildUrlDeserializer"/> class.
    /// </summary>
    public static class BuildConverter
    {
        private static ISet<TryCreateDeserializer> _deserializersFactories = new HashSet<TryCreateDeserializer>();
        private static Func<string, BuildUrlDeserializer> _factory;

        /// <summary>
        /// Registers default deserializer that should be used if all others refused to parse.
        /// It may be useful when trying to parse obsolete build urls and official site compatible urls.
        /// </summary>
        /// <param name="factory">The default deserializer factory.</param>
        public static void RegisterDefaultDeserializer(Func<string, BuildUrlDeserializer> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Registers deserializers factory methods ignoring duplications. When the converter creates a deserializer, factories are called sequentially
        /// in the same order as registered. If no one matches, default deserializer is used.
        /// </summary>
        /// <param name="factories">The collection of factory methods.</param>
        public static void RegisterDeserializersFactories(params TryCreateDeserializer[] factories)
        {
            _deserializersFactories = new HashSet<TryCreateDeserializer>(factories);
        }

        /// <summary>
        /// Creates an instance of a class derived from the <see cref="BuildUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <returns>An instance of the deserializer corresponding to the specified <paramref name="buildUrl"/>.</returns>
        public static BuildUrlDeserializer GetUrlDeserializer(string buildUrl)
        {
            foreach (var tryCreateDelegate in _deserializersFactories)
            {
                BuildUrlDeserializer deserializer;
                if (tryCreateDelegate(buildUrl, out deserializer))
                {
                    return deserializer;
                }
            }

            return _factory?.Invoke(buildUrl);
        }

        /// <summary>
        /// Factory method delegate, representing method to create an instance of a url deserializer.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <param name="deserializer">When this method returns, contains the deserializer instance or null, if url conversion is impossible.</param>
        /// <returns>true if deserializer was created successfully; otherwise, false.</returns>
        public delegate bool TryCreateDeserializer(string buildUrl, out BuildUrlDeserializer deserializer);
    }
}