using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel
{
    /// <summary>
    /// Deserializes data files asynchronously and contains related constants.
    /// </summary>
    public static class DataUtils
    {
        private const string ResourceRoot = "PoESkillTree.GameModel.Data.";
        public const string RePoEDataUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/";
        public const string RePoEFileSuffix = ".min.json";

        public static Task<T> LoadRePoEAsync<T>(string fileName, bool deserializeOnThreadPool = false)
            => LoadJsonAsync<T>(RePoEFileToResource(fileName), deserializeOnThreadPool);

        public static Task<JObject> LoadRePoEAsObjectAsync(string fileName, bool loadOnThreadPool = false)
        {
            var reader = CreateResourceStreamReader(RePoEFileToResource(fileName));
            return JsonSerializationUtils.LoadJObjectAsync(reader, loadOnThreadPool);
        }

        private static string RePoEFileToResource(string fileName)
            => "RePoE." + fileName.Replace("/", ".") + RePoEFileSuffix;

        public static Task<T> LoadJsonAsync<T>(string fileName, bool deserializeOnThreadPool = false)
            => JsonSerializationUtils.DeserializeAsync<T>(CreateResourceStreamReader(fileName),
                deserializeOnThreadPool);

        public static Task<T> LoadXmlAsync<T>(string fileName, bool deserializeOnThreadPool = false)
            => XmlSerializationUtils.DeserializeAsync<T>(CreateResourceStreamReader(fileName), deserializeOnThreadPool);

        private static StreamReader CreateResourceStreamReader(string resourceName)
        {
            var name = ResourceRoot + resourceName;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream is null)
                throw new ArgumentException("Unknown resource " + name);
            return new StreamReader(stream);
        }
    }
}