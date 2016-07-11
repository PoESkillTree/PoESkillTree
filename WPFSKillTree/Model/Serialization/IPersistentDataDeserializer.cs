using System;
using JetBrains.Annotations;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Interface for a class that can deserialize PersistentData for a specific version range.
    /// </summary>
    public interface IPersistentDataDeserializer
    {
        /// <summary>
        /// The minimum version this instance can deserialize. <c>null</c> indicates any version, include
        /// no version being specified at all.
        /// </summary>
        [CanBeNull]
        Version MinimumDeserializableVersion { get; }

        /// <summary>
        /// The maxmimum version this instance can deserialize. <c>null</c> indicates that this instance can only
        /// deserialize if there was no version specified.
        /// </summary>
        [CanBeNull]
        Version MaximumDeserializableVersion { get; }

        /// <summary>
        /// Deserializes and creates a <see cref="PersistentData"/> instance.
        /// </summary>
        /// <param name="xmlString">The contents of the PersistentData.xml file. Other files than this may be
        /// used additionally.</param>
        /// <returns>The deserialized, fully initialized <see cref="PersistentData"/> instance.</returns>
        PersistentData Deserialize(string xmlString);

        /// <summary>
        /// Creates a <see cref="PersistentData"/> instance not based on a PersistentData.xml file.
        /// </summary>
        /// <returns></returns>
        PersistentData CreateDefaultPersistentData();
    }
}