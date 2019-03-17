using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace PoESkillTree.Utils
{
    /// <summary>
    /// Contains build serialization related utility methods.
    /// </summary>
    public static class SerializationUtils
    {
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