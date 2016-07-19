using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace POESKillTree.Utils
{
    public static class SerializationUtils
    {
        public static async Task<T> DeserializeFileAsync<T>(string filePath)
        {
            return DeserializeString<T>(await FileEx.ReadAllTextAsync(filePath));
        }

        public static T DeserializeString<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        public static void Serialize<T>(T obj, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
            }
        }

        public static string AssemblyFileVersion { get; } = GetAssemblyFileVersion();

        private static string GetAssemblyFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static string DecodeFileName(string fileName)
        {
            return WebUtility.UrlDecode(fileName);
        }

        public static string EncodeFileName(string fileName)
        {
            // * (asterisk) is not encoded but is not allowed in Windows file names
            return WebUtility.UrlEncode(fileName)?.Replace("*", "%2a");
        }
    }
}