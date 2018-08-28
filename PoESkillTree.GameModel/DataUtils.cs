using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PoESkillTree.GameModel
{
    /// <summary>
    /// Deserializes data files and contains related constants.
    /// </summary>
    public static class DataUtils
    {
        private const string ResourceRoot = "PoESkillTree.GameModel.Data.";
        public const string RePoEDataUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/";
        public const string RePoEFileSuffix = ".min.json";

        /// <summary>
        /// Asynchronously deserializes the data file with the given name (without extension).
        /// </summary>
        /// <typeparam name="T">type to deserialize the json as</typeparam>
        /// <param name="fileName">the data file to deserialize</param>
        /// <returns>a task returning the deserialized object</returns>
        public static async Task<T> LoadRePoEAsync<T>(string fileName)
        {
            var text = await LoadTextAsync("RePoE." + fileName + RePoEFileSuffix).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static async Task<string> LoadTextAsync(string resourceName)
        {
            var name = ResourceRoot + resourceName;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                if (stream is null)
                {
                    throw new ArgumentException("Unknown resource " + name);
                }
                using (var reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }
    }
}