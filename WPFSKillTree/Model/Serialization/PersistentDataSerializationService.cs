using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using POESKillTree.Utils;

using static POESKillTree.Model.Serialization.SerializationUtils;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Provides services for loading and saving <see cref="IPersistentData"/> instances from/to the file system.
    /// </summary>
    public class PersistentDataSerializationService
    {
        private const string FileName = "PersistentData";

        private static readonly ILog Log = LogManager.GetLogger(typeof(PersistentDataSerializationService));

        private readonly IReadOnlyList<IPersistentDataDeserializer> _deserializer = new IPersistentDataDeserializer[]
        {
            new PersistentDataDeserializerUpTo2210(),
            new PersistentDataDeserializerCurrent()
        };
        private readonly PersistentDataSerializer _serializer = new PersistentDataSerializer();

        private readonly string _filePath = AppData.GetFolder(true) + FileName + ".xml";
        private readonly string _backupPath = AppData.GetFolder(true) + FileName + ".bak";

        public IPersistentData Deserialize()
        {
            var data = DeserializeCore();
            data.RequestsSave += (sender, args) => Serialize(data);
            return data;
        }

        private IPersistentData DeserializeCore()
        {
            if (!File.Exists(_filePath))
                return new PersistentData();

            try
            {
                return Deserialize(_filePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(_backupPath))
                    return Deserialize(_backupPath);

                var badFilePath = AppData.GetFolder(true) + FileName + "_Bad.xml";
                File.Copy(_filePath, badFilePath, true);

                FileEx.DeleteIfExists(_filePath);
                FileEx.DeleteIfExists(_backupPath);

                Log.Error("Could not deserialize PeristentData file", ex);
                throw new Exception(ex.Message +
                                    "\nYour PersistentData file could not be loaded correctly. It has been moved to " +
                                    badFilePath);
            }
        }

        private IPersistentData Deserialize(string filePath)
        {
            string xmlString;
            using (var reader = new StreamReader(filePath))
            {
                xmlString = reader.ReadToEnd();
            }
            var version = DeserializeStringAs<XmlPersistentDataVersion>(xmlString).AppVersion;
            IPersistentDataDeserializer suitableDeserializer;
            if (version == null)
            {
                suitableDeserializer = _deserializer.FirstOrDefault(c => c.MinimumDeserializableVersion == null);
            }
            else
            {
                var v = new Version(version);
                suitableDeserializer = _deserializer.FirstOrDefault(c =>
                    v.CompareTo(c.MinimumDeserializableVersion) >= 0
                    && v.CompareTo(c.MaximumDeserializableVersion) <= 0);
            }
            if (suitableDeserializer == null)
            {
                throw new Exception(
                    $"No converter available that can deserialize a PersistentData file with version {version}");
            }
            return suitableDeserializer.Deserialize(xmlString);
        }

        public void Serialize(IPersistentData persistentData)
        {
            FileEx.MoveIfExists(_filePath, _backupPath, true);
            _serializer.Serialize(persistentData, _filePath);
        }

        // Creates empty file with language option set.
        [UsedImplicitly]
        public static void CreateSetupTemplate(string path, string language)
        {
            var data = new PersistentData { Options = { Language = language } };
            var manager = new PersistentDataSerializationService();
            manager._serializer.Serialize(data, Path.Combine(path, FileName + ".xml"));
        }
    }
}