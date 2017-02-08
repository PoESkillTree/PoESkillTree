using System;
using System.Collections.Generic;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Utils.UrlProcessing
{
    public delegate bool TryCreateDeserializer(string buildUrl, IAscendancyClasses ascendancyClasses, out BuildUrlDeserializer deserializer);

    /// <summary>
    /// Creates instances of classes derived from the <see cref="BuildUrlDeserializer"/> class.
    /// </summary>
    public class BuildConverter : IBuildConverter
    {
        private readonly IAscendancyClasses _ascendancyClasses;

        private ISet<TryCreateDeserializer> _deserializersFactories = new HashSet<TryCreateDeserializer>();
        private Func<string, BuildUrlDeserializer> _factory;

        public BuildConverter(IAscendancyClasses ascendancyClasses)
        {
            _ascendancyClasses = ascendancyClasses;
        }

        public void RegisterDefaultDeserializer(Func<string, BuildUrlDeserializer> factory)
        {
            _factory = factory;
        }

        public void RegisterDeserializersFactories(params TryCreateDeserializer[] factories)
        {
            _deserializersFactories = new HashSet<TryCreateDeserializer>(factories);
        }

        public BuildUrlDeserializer GetUrlDeserializer(string buildUrl)
        {
            foreach (var tryCreateDelegate in _deserializersFactories)
            {
                BuildUrlDeserializer deserializer;
                if (tryCreateDelegate(buildUrl, _ascendancyClasses, out deserializer))
                {
                    return deserializer;
                }
            }

            return _factory?.Invoke(buildUrl);
        }
    }
}