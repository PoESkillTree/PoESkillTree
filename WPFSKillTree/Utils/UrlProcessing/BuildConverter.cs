using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PoESkillTree.SkillTreeFiles;

namespace PoESkillTree.Utils.UrlProcessing
{
    public delegate bool TryCreateDeserializer(
        string buildUrl, IAscendancyClasses ascendancyClasses,
        [NotNullWhen(true)] out BuildUrlDeserializer? deserializer);

    /// <summary>
    /// Creates instances of classes derived from the <see cref="BuildUrlDeserializer"/> class.
    /// </summary>
    public class BuildConverter : IBuildConverter
    {
        private readonly IAscendancyClasses _ascendancyClasses;

        private readonly ISet<TryCreateDeserializer> _deserializersFactories;
        private readonly Func<string, BuildUrlDeserializer> _factory;

        public BuildConverter(
            IAscendancyClasses ascendancyClasses,
            Func<string, BuildUrlDeserializer> factory,
            params TryCreateDeserializer[] deserializersFactories)
        {
            _ascendancyClasses = ascendancyClasses;
            _deserializersFactories = new HashSet<TryCreateDeserializer>(deserializersFactories);
            _factory = factory;
        }

        public BuildUrlDeserializer GetUrlDeserializer(string buildUrl)
        {
            foreach (var tryCreateDelegate in _deserializersFactories)
            {
                if (tryCreateDelegate(buildUrl, _ascendancyClasses, out var deserializer))
                {
                    return deserializer;
                }
            }

            return _factory.Invoke(buildUrl);
        }
    }
}