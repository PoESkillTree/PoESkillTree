using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json.Linq;
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

        protected PersistentData PersistentData { get; private set; }

        protected AbstractPersistentDataDeserializer(string minimumConvertableVersion, string maximumConvertableVersion)
        {
            if (minimumConvertableVersion != null)
                MinimumDeserializableVersion = new Version(minimumConvertableVersion);
            if (maximumConvertableVersion != null)
                MaximumDeserializableVersion = new Version(maximumConvertableVersion);
        }

        public IPersistentData Deserialize(string xmlString)
        {
            PersistentData = new PersistentData();
            DeserializePersistentDataFile(xmlString);
            PersistentData.EquipmentData = DeserializeEquipmentData();
            PersistentData.StashItems.AddRange(DeserializeStashItems());
            return PersistentData;
        }

        protected abstract void DeserializePersistentDataFile(string xmlString);

        private EquipmentData DeserializeEquipmentData()
        {
            return new EquipmentData(PersistentData.Options);
        }

        private IEnumerable<Item> DeserializeStashItems()
        {
            try
            {
                var file = Path.Combine(AppData.GetFolder(), "stash.json");
                if (File.Exists(file))
                    return JArray.Parse(File.ReadAllText(file)).Select(item => new Item(PersistentData, (JObject) item));
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