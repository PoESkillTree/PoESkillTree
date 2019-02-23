using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var resourceName = RePoEFileToResource(fileName);
            if (loadOnThreadPool)
                return Task.Run(async () => await LoadJObjectAsync(resourceName));
            return LoadJObjectAsync(resourceName);
        }

        private static string RePoEFileToResource(string fileName)
            => "RePoE." + fileName.Replace("/", ".") + RePoEFileSuffix;

        public static async Task<T> LoadJsonAsync<T>(string fileName, bool deserializeOnThreadPool = false)
        {
            var text = await LoadTextAsync(fileName).ConfigureAwait(false);
            if (deserializeOnThreadPool)
                return await Task.Run(() => JsonConvert.DeserializeObject<T>(text)).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static async Task<T> LoadXmlAsync<T>(string fileName, bool deserializeOnThreadPool = false)
        {
            var xmlString = await LoadTextAsync(fileName).ConfigureAwait(false);
            if (deserializeOnThreadPool)
                return await Task.Run(() => Deserialize(xmlString)).ConfigureAwait(false);
            return Deserialize(xmlString);

            T Deserialize(string s)
            {
                using (var reader = new StringReader(s))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    return (T) serializer.Deserialize(reader);
                }
            }
        }

        private static async Task<string> LoadTextAsync(string resourceName)
        {
            using (var reader = CreateResourceStreamReader(resourceName))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private static async Task<JObject> LoadJObjectAsync(string resourceName)
        {
            using (var reader = CreateResourceStreamReader(resourceName))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return await JObject.LoadAsync(jsonReader).ConfigureAwait(false);
            }
        }

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