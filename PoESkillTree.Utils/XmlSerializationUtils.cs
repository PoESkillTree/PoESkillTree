using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PoESkillTree.Utils
{
    public static class XmlSerializationUtils
    {
        /// <summary>
        /// Deserializes an object of type <typeparamref name="T"/> from <paramref name="filePath"/>
        /// asynchronously using XmlSerializer.
        /// </summary>
        public static Task<T> DeserializeFileAsync<T>(string filePath, bool deserializeOnThreadPool = false)
            => DeserializeAsync<T>(File.OpenText(filePath), deserializeOnThreadPool);

        public static async Task<T> DeserializeAsync<T>(TextReader reader, bool deserializeOnThreadPool = false)
        {
            var xmlString = await FileUtils.ReadAndDisposeAsync(reader).ConfigureAwait(false);
            if (deserializeOnThreadPool)
                return await Task.Run(() => DeserializeString<T>(xmlString)).ConfigureAwait(false);
            return DeserializeString<T>(xmlString);
        }

        /// <summary>
        /// Deserializes an object of type <typeparamref name="T"/> from the given string using XmlSerializer.
        /// </summary>
        public static T DeserializeString<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T) serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serializes <paramref name="obj"/> to <paramref name="filePath"/> using XmlSerializer.
        /// </summary>
        public static void SerializeToFile<T>(T obj, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                SerializeToWriter(obj, writer);
            }
        }

        /// <summary>
        /// Serializes <paramref name="obj"/> to a string using XmlSerializer.
        /// </summary>
        public static string SerializeToString<T>(T obj)
        {
            using (var writer = new StringWriter())
            {
                SerializeToWriter(obj, writer);
                return writer.ToString();
            }
        }

        private static void SerializeToWriter<T>(T obj, TextWriter writer)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj);
        }
    }
}