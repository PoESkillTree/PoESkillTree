using System.Threading.Tasks;
using Newtonsoft.Json;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Deserializes json data files from RePoE from the file system and contains related constants.
    /// </summary>
    public static class RePoEUtils
    {
        public const string RePoEDataUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/";
        public const string FileSuffix = ".min.json";
        private const string ResourcePath =
            "pack://application:,,,/PoESkillTree;component/Data/RePoE/";

        static RePoEUtils()
        {
            Util.TriggerPackUriSchemeInitialization();
        }

        /// <summary>
        /// Asynchronously deserializes the data file with the given name (without extension).
        /// </summary>
        /// <typeparam name="T">type to deserialize the json as</typeparam>
        /// <param name="fileName">the data file to deserialize</param>
        /// <returns>a task returning the deserialized object</returns>
        public static async Task<T> LoadAsync<T>(string fileName)
        {
            var path = ResourcePath + fileName + FileSuffix;
            var text = await SerializationUtils.ReadResourceAsync(path);
            return JsonConvert.DeserializeObject<T>(text);
        }
    }
}