using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PoESkillTree.Controls.Dialogs;

namespace PoESkillTree.Model.Serialization
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
        /// The maximum version this instance can deserialize. <c>null</c> indicates that this instance can only
        /// deserialize if there was no version specified.
        /// </summary>
        [CanBeNull]
        Version MaximumDeserializableVersion { get; }

        /// <summary>
        /// Sets the <see cref="AbstractPersistentData"/> this deserializer works on. Should not be set after
        /// <see cref="DeserializePersistentDataFile"/> has been called. <see cref="IPersistentData.SaveBuild"/>
        /// may only be called after <see cref="InitializeAsync"/>.
        /// </summary>
        AbstractPersistentData PersistentData { set; }

        /// <summary>
        /// Deserializes PersistentData.xml and sets it in <see cref="PersistentData"/>
        /// </summary>
        /// <param name="xmlString">The contents of PersistentData.xml.</param>
        void DeserializePersistentDataFile(string xmlString);

        /// <summary>
        /// Asynchronously initializes all properties of <see cref="PersistentData"/> not contained in
        /// PersistentData.xml.
        /// </summary>
        Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        /// <summary>
        /// Must be called after <see cref="InitializeAsync"/> to save all build changes done before.
        /// </summary>
        void SaveBuildChanges();
    }
}