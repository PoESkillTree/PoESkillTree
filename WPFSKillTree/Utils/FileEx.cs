using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace POESKillTree.Utils
{
    public static class FileEx
    {
        public static async Task<string> ReadAllTextAsync(string path)
        {
            using (var reader = File.OpenText(path))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        public static async Task WriteAllTextAsync(string path, string contents)
        {
            using (var writer = File.CreateText(path))
            {
                await writer.WriteAsync(contents).ConfigureAwait(false);
            }
        }

        public static async Task WriteStreamAsync(string path, Stream contents)
        {
            using (var writer = File.Create(path))
            {
                await contents.CopyToAsync(writer).ConfigureAwait(false);
            }
        }

        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static void CopyIfExists(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (File.Exists(sourceFileName))
                File.Copy(sourceFileName, destFileName, overwrite);
        }

        public static void MoveOverwriting(string sourceFileName, string destFileName)
        {
            DeleteIfExists(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        public static void MoveIfExists(string sourceFileName, string destFileName, bool overwrite = false)
        {
            if (!File.Exists(sourceFileName))
                return;
            if (overwrite)
                DeleteIfExists(destFileName);
            File.Move(sourceFileName, destFileName);
        }

        private static Dictionary<string, IEnumerable<string>> assemblyDict = new Dictionary<string, IEnumerable<string>>();

        public static string GetResource<TBase>(string resourceName)
        {
            return GetResource(Assembly.GetAssembly(typeof(TBase)), resourceName);
        }
        private static string GetResource(Assembly assembly, string resourceName)
        {
            var rn = resourceName.Replace("/", ".").Replace("\\", ".");
            var resourceNames = GetManifestNames(assembly);
            var extantResourceName = resourceNames.FirstOrDefault(x => string.Equals(rn, x, System.StringComparison.InvariantCultureIgnoreCase));
            if (extantResourceName == null)
                throw new FileNotFoundException($"{rn} not found.");
            return GetResourceText(assembly, extantResourceName);
        }

        private static IEnumerable<string> GetManifestNames(Assembly assembly)
        {
            if (!assemblyDict.ContainsKey(assembly.FullName))
                assemblyDict[assembly.FullName] = assembly.GetManifestResourceNames();
            return assemblyDict[assembly.FullName];
        }

        private static string GetResourceText(Assembly assembly, string resourceName)
        {
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}