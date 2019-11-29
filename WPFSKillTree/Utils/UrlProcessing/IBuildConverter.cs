namespace PoESkillTree.Utils.UrlProcessing
{
    /// <summary>
    /// Creates instances of classes derived from the <see cref="BuildUrlDeserializer"/> class.
    /// </summary>
    public interface IBuildConverter
    {
        /// <summary>
        /// Creates an instance of a class derived from the <see cref="BuildUrlDeserializer"/> class.
        /// </summary>
        /// <param name="buildUrl">The PoE build url.</param>
        /// <returns>An instance of the deserializer corresponding to the specified <paramref name="buildUrl"/>.</returns>
        BuildUrlDeserializer GetUrlDeserializer(string buildUrl);
    }
}