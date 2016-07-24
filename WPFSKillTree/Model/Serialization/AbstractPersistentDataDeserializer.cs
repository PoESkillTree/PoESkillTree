using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Abstract implementation of <see cref="IPersistentDataDeserializer"/> providing logic used by multiple
    /// subclasses.
    /// </summary>
    public abstract class AbstractPersistentDataDeserializer : IPersistentDataDeserializer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AbstractPersistentDataDeserializer));

        public Version MinimumDeserializableVersion { get; }
        public Version MaximumDeserializableVersion { get; }

        public AbstractPersistentData PersistentData { protected get; set; }

        protected AbstractPersistentDataDeserializer(string minimumConvertableVersion, string maximumConvertableVersion)
        {
            if (minimumConvertableVersion != null)
                MinimumDeserializableVersion = new Version(minimumConvertableVersion);
            if (maximumConvertableVersion != null)
                MaximumDeserializableVersion = new Version(maximumConvertableVersion);
        }

        public abstract void DeserializePersistentDataFile(string xmlString);

        public async Task InitializeAsync(IDialogCoordinator dialogCoordinator)
        {
            if (PersistentData.Options.BuildsSavePath == null)
            {
                if (AppData.IsPortable)
                {
                    PersistentData.Options.BuildsSavePath = AppData.GetFolder("Builds");
                }
                else
                {
                    // Ask user for path. Default: AppData.GetFolder("Builds")
                    PersistentData.Options.BuildsSavePath = await dialogCoordinator.ShowFileSelectorAsync(PersistentData,
                        L10n.Message("Select build directory"),
                        L10n.Message("Select the directory where builds will be stored.\n" +
                                     "It will be created if it does not yet exist."),
                        AppData.GetFolder("Builds"), true);
                }
            }
            await DeserializeAdditionalFilesAsync();
            PersistentData.EquipmentData = await DeserializeEquipmentData();
            PersistentData.StashItems.AddRange(await DeserializeStashItemsAsync());
        }

        protected abstract Task DeserializeAdditionalFilesAsync();

        private Task<EquipmentData> DeserializeEquipmentData()
        {
            return EquipmentData.CreateAsync(PersistentData.Options);
        }

        private async Task<IEnumerable<Item>> DeserializeStashItemsAsync()
        {
            try
            {
                var file = Path.Combine(AppData.GetFolder(), "stash.json");
                if (File.Exists(file))
                    return JArray.Parse(await FileEx.ReadAllTextAsync(file)).Select(item => new Item(PersistentData, (JObject) item));
            }
            catch (Exception e)
            {
                Log.Error("Could not deserialize stash", e);
            }
            return Enumerable.Empty<Item>();
        }

        protected static PoEBuild CreateDefaultCurrentBuild()
        {
            return new PoEBuild { Name = "New Build" };
        }
    }
}