using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Abstract implementation of <see cref="IPersistentDataDeserializer"/> providing logic used by multiple
    /// subclasses.
    /// </summary>
    public abstract class AbstractPersistentDataDeserializer : IPersistentDataDeserializer
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public Version? MinimumDeserializableVersion { get; }
        public Version? MaximumDeserializableVersion { get; }

        public AbstractPersistentData PersistentData { protected get; set; }

        /// <summary>
        /// Gets a <see cref="IDialogCoordinator"/> instance that can be used for showing dialogs. Only set after
        /// (or while) <see cref="InitializeAsync"/> has been called. <see cref="PersistentData"/> is used as context
        /// instance.
        /// </summary>
        protected IDialogCoordinator DialogCoordinator { get; private set; }

        /// <summary>
        /// Gets or sets whether this <see cref="AbstractPersistentDataDeserializer"/> subclass deserializes files
        /// located under BuildsSavePath.
        /// </summary>
        protected bool DeserializesBuildsSavePath { private get; set; }

#pragma warning disable CS8618 // PersistentData has to be set before and DialogCoordinator will be set in InitializeAsnyc
        protected AbstractPersistentDataDeserializer(string? minimumConvertableVersion, string? maximumConvertableVersion)
#pragma warning restore
        {
            if (minimumConvertableVersion != null)
                MinimumDeserializableVersion = new Version(minimumConvertableVersion);
            if (maximumConvertableVersion != null)
                MaximumDeserializableVersion = new Version(maximumConvertableVersion);
        }

        public abstract void DeserializePersistentDataFile(string xmlString);

        /// <summary>
        /// Gets the longest subpath of BuildsSavePath that must be serializable.
        /// </summary>
        protected virtual string GetLongestRequiredSubpath()
        {
            return SerializationConstants.EncodedDefaultBuildName;
        }

        public async Task InitializeAsync(IDialogCoordinator dialogCoordinator)
        {
            DialogCoordinator = dialogCoordinator;
            if (string.IsNullOrEmpty(PersistentData.Options.BuildsSavePath))
            {
                PersistentData.Options.BuildsSavePath = await GetBuildsSavePathAsync();
            }
            Directory.CreateDirectory(PersistentData.Options.BuildsSavePath);

            var equipmentDataTask = DeserializeEquipmentDataAsync();
            var additionalFilesTask = DeserializeAdditionalFilesAsync();
            PersistentData.EquipmentData = await equipmentDataTask;
            PersistentData.StashItems.AddRange(await DeserializeStashItemsAsync());
            await additionalFilesTask;
        }

        private async Task<string> GetBuildsSavePathAsync()
        {
            if (AppData.IsPortable)
                return AppData.ToRelativePath(AppData.GetFolder("Builds"));

            // Ask user for path. Default: AppData.GetFolder("Builds")
            var dialogSettings = new FileSelectorDialogSettings
            {
                DefaultPath = AppData.GetFolder("Builds"),
                IsFolderPicker = true,
                ValidationSubPath = GetLongestRequiredSubpath(),
                IsCancelable = false
            };
            if (!DeserializesBuildsSavePath)
            {
                dialogSettings.AdditionalValidationFunc =
                    path => Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any()
                        ? L10n.Message("Directory must be empty.")
                        : null;
            }
            return (await DialogCoordinator.ShowFileSelectorAsync(PersistentData,
                L10n.Message("Select build directory"),
                L10n.Message("Select the directory where builds will be stored.\n" +
                             "It will be created if it does not yet exist. You can change it in the settings later."),
                dialogSettings))!;
        }

        public virtual void SaveBuildChanges()
        {
        }

        /// <summary>
        /// Deserializes files other than PersistentData.xml asynchronously. Called in <see cref="InitializeAsync"/>.
        /// </summary>
        protected abstract Task DeserializeAdditionalFilesAsync();

        private Task<EquipmentData> DeserializeEquipmentDataAsync()
            => EquipmentData.CreateAsync(PersistentData.Options);

        private async Task<IEnumerable<Item>> DeserializeStashItemsAsync()
        {
            try
            {
                var file = Path.Combine(AppData.GetFolder(), "stash.json");
                if (File.Exists(file))
                {
                    var jArray = await JsonSerializationUtils.DeserializeJArrayFromFileAsync(file, true)
                        .ConfigureAwait(false);
                    return jArray.Select(item => new Item(PersistentData.EquipmentData, (JObject) item));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Could not deserialize stash");
            }
            return Enumerable.Empty<Item>();
        }

        /// <summary>
        /// Creates and returns a <see cref="PoEBuild"/> instance with the default build name.
        /// </summary>
        /// <returns></returns>
        protected static PoEBuild CreateDefaultCurrentBuild()
        {
            return new PoEBuild { Name = SerializationConstants.DefaultBuildName };
        }

        /// <summary>
        /// Creates a new <see cref="PoEBuild"/> instance from the given <see cref="XmlBuild"/> instance.
        /// Will return null if null is given.
        /// </summary>
        [return: NotNullIfNotNull("build")]
        protected static PoEBuild? ConvertFromXmlBuild(XmlBuild? build)
        {
            if (build == null)
                return null;
            return new PoEBuild(build.Bandits, build.CustomGroups, build.CheckedNodeIds, build.CrossedNodeIds,
                build.ConfigurationStats, build.AdditionalData)
            {
                AccountName = build.AccountName,
                CharacterName = build.CharacterName,
                ItemData = build.ItemData,
                LastUpdated = build.LastUpdated,
                League = build.League,
                Realm = build.Realm,
                Level = build.Level,
                Name = build.Name,
                Note = build.Note,
                TreeUrl = build.TreeUrl
            };
        }
    }
}