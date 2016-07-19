using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using POESKillTree.Utils;

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
        private readonly Dictionary<IPersistentData, PersistentDataSerializer> _serializerDict =
            new Dictionary<IPersistentData, PersistentDataSerializer>();

        private readonly string _filePath = AppData.GetFolder(true) + FileName + ".xml";
        private readonly string _backupPath = AppData.GetFolder(true) + FileName + ".bak";

        public IPersistentData Deserialize()
        {
            var data = DeserializeCore();
            var serializer = new PersistentDataSerializer(data);
            _serializerDict[data] = serializer;
            data.SaveHandler += () => Serialize(data);
            data.SaveBuildHandler += serializer.SerializeBuild;
            data.DeleteBuildHandler += serializer.DeleteBuild;
            data.Initialized += () => serializer.SerializeAllBuilds();
            return data;
        }

        private PersistentData DeserializeCore()
        {
            if (!File.Exists(_filePath))
                return _deserializer.Last().CreateDefaultPersistentData();

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

        private PersistentData Deserialize(string filePath)
        {
            var xmlString = File.ReadAllText(filePath);
            var version = SerializationUtils.DeserializeString<XmlPersistentDataVersion>(xmlString).AppVersion;
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
            _serializerDict[persistentData].Serialize(_filePath);
        }

        // Creates empty file with language option set.
        [UsedImplicitly]
        public static void CreateSetupTemplate(string path, string language)
        {
            var data = new PersistentData { Options = { Language = language } };
            new PersistentDataSerializer(data).Serialize(Path.Combine(path, FileName + ".xml"));
        }
    }
}