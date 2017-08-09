using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Contains utility methods for serialization and deserialization.
    /// </summary>
    public static class SerializationUtils
    {
        /// <summary>
        /// Asynchronously reads the resource at the given path as a string.
        /// </summary>
        public static async Task<string> ReadResourceAsync(string path)
        {
            var resource = Application.GetResourceStream(new Uri(path));
            using (var stream = resource.Stream)
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Deserializes an object of type <typeparamref name="T"/> from <paramref name="filePath"/>
        /// asynchronously using XmlSerializer.
        /// </summary>
        public static async Task<T> XmlDeserializeFileAsync<T>(string filePath)
        {
            return XmlDeserializeString<T>(await FileEx.ReadAllTextAsync(filePath));
        }

        /// <summary>
        /// Deserializes an object of type <typeparamref name="T"/> from the given string using XmlSerializer.
        /// </summary>
        public static T XmlDeserializeString<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Serializes <paramref name="obj"/> to <paramref name="filePath"/> using XmlSerializer.
        /// </summary>
        public static void XmlSerializeToFile<T>(T obj, string filePath)
        {
            using (var writer = new StreamWriter(filePath))
            {
                XmlSerializeToWriter(obj, writer);
            }
        }

        /// <summary>
        /// Serializes <paramref name="obj"/> to a string using XmlSerializer.
        /// </summary>
        public static string XmlSerializeToString<T>(T obj)
        {
            using (var writer = new StringWriter())
            {
                XmlSerializeToWriter(obj, writer);
                return writer.ToString();
            }
        }

        private static void XmlSerializeToWriter<T>(T obj, TextWriter writer)
        {
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj);
        }

        /// <summary>
        /// The <see cref="FileVersionInfo.FileVersion"/> of the containing assembly.
        /// </summary>
        public static readonly string AssemblyFileVersion = GetAssemblyFileVersion();

        private static string GetAssemblyFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        /// <summary>
        /// Decodes the given file name that was encoded with <see cref="EncodeFileName"/>.
        /// </summary>
        public static string DecodeFileName(string fileName)
        {
            return WebUtility.UrlDecode(fileName);
        }

        /// <summary>
        /// Encodes the given file name so that it contains no characters that are invalid as file names.
        /// </summary>
        public static string EncodeFileName(string fileName)
        {
            // * (asterisk) is not encoded but is not allowed in Windows file names
            // . (full stop) is silently removed at the end of folder names
            return WebUtility.UrlEncode(fileName)?.Replace("*", "%2a").Replace(".", "%2e");
        }
    }
}