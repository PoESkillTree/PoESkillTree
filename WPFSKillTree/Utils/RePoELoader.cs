using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Downloads json data files from RePoE and saves/deserializes them to/from the file system.
    /// </summary>
    public class RePoELoader
    {
        private const string RePoEDataUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/";
        private const string FileSuffix = ".min.json";
        private static readonly string DataPath = AppData.GetFolder(Path.Combine("Data", "RePoE"));

        private readonly HttpClient _httpClient;
        private readonly bool _overwrite;

        /// <param name="httpClient">The client to use for downloads</param>
        /// <param name="overwrite">True if files should always be downloaded and overwritten if they exist</param>
        public RePoELoader(HttpClient httpClient, bool overwrite)
        {
            _httpClient = httpClient;
            _overwrite = overwrite;
        }

        /// <summary>
        /// Asynchronously deserializes the data file with the given name (without extension).
        /// If the file does not exists or the overwrite constructor parameter was set to true, it will be
        /// downloaded from the RePoE repository.
        /// </summary>
        /// <typeparam name="T">type to deserialize the json as</typeparam>
        /// <param name="fileName">the data file to deserialize</param>
        /// <returns>a task returning the deserialized object</returns>
        public async Task<T> LoadAsync<T>(string fileName)
        {
            var file = fileName + FileSuffix;
            var filePath = Path.Combine(DataPath, file);
            if (_overwrite || !File.Exists(filePath))
            {
                var stream = await _httpClient.GetStreamAsync(RePoEDataUrl + file);
                await FileEx.WriteStreamAsync(filePath, stream);
            }
            var fileContents = await FileEx.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(fileContents);
        }
    }
}