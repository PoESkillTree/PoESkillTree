using System;

namespace PoESkillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Creates instances of classes derived from the <see cref="BuildUrlDeserializer"/> class.
    /// </summary>
    public interface IBuildConverter
    {
        /// <summary>
        /// Registers default deserializer that should be used if all others refused to parse.
        /// It may be useful when trying to parse obsolete build urls and official site compatible urls.
        /// </summary>
        /// <param name="factory">The default deserializer factory.</param>
        void RegisterDefaultDeserializer(Func<string, BuildUrlDeserializer> factory);

        /// <summary>
        /// Registers deserializers factory methods ignoring duplications. When the converter creates a deserializer, factories are called sequentially
        /// in the same order as registered. If no one matches, default deserializer is used.
        /// </summary>
        /// <param name="factories">The collection of factory methods.</param>
        void RegisterDeserializersFactories(params TryCreateDeserializer[] factories);

        /// <summary>
        /// Creates an instance of a class derived from the <see cref="BuildUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <returns>An instance of the deserializer corresponding to the specified <paramref name="buildUrl"/>.</returns>
        BuildUrlDeserializer GetUrlDeserializer(string buildUrl);
    }
}