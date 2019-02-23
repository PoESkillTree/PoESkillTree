using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoESkillTree.Utils
{
    public static class JsonSerializationUtils
    {
        public static async Task<T> DeserializeAsync<T>(TextReader reader, bool deserializeOnThreadPool = false)
        {
            var text = await FileUtils.ReadAndDisposeAsync(reader).ConfigureAwait(false);
            if (deserializeOnThreadPool)
                return await Task.Run(() => JsonConvert.DeserializeObject<T>(text)).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static Task<JArray> DeserializeJArrayFromFileAsync(string filePath, bool onThreadPool = false)
            => LoadJArrayAsync(File.OpenText(filePath), onThreadPool);

        public static Task<JObject> LoadJObjectAsync(TextReader reader, bool onThreadPool = false)
            => LoadJLinqAsync(reader, onThreadPool, r => JObject.LoadAsync(r));

        public static Task<JArray> LoadJArrayAsync(TextReader reader, bool onThreadPool = false)
            => LoadJLinqAsync(reader, onThreadPool, r => JArray.LoadAsync(r));

        private static Task<T> LoadJLinqAsync<T>(TextReader reader, bool onThreadPool, Func<JsonReader, Task<T>> load)
        {
            if (onThreadPool)
                return Task.Run(async () => await Load(reader));
            return Load(reader);

            async Task<T> Load(TextReader r)
            {
                using (r)
                using (var jsonReader = new JsonTextReader(r))
                {
                    return await load(jsonReader).ConfigureAwait(false);
                }
            }
        }
    }
}