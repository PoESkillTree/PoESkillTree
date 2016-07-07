using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace POESKillTree.Model.Serialization
{
    public static class SerializationUtils
    {
        public static T DeserializeStringAs<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
            {
                return DeserializeAs<T>(reader);
            }
        }

        public static T DeserializeFileAs<T>(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                return DeserializeAs<T>(reader);
            }
        }

        public static T DeserializeAs<T>(TextReader reader)
        {
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }

        public static void Serialize<T>(T obj, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, obj);
            }
        }

        public static string GetAssemblyFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }
    }
}