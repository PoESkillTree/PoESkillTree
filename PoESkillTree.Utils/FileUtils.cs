using System.IO;
using System.Threading.Tasks;

namespace PoESkillTree.Utils
{
    public static class FileUtils
    {
        public static Task<string> ReadAllTextAsync(string path)
            => ReadAndDisposeAsync(File.OpenText(path));

        public static async Task<string> ReadAndDisposeAsync(TextReader reader)
        {
            using (reader)
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        public static async Task WriteAllTextAsync(string path, string contents)
        {
            using (var writer = File.CreateText(path))
            {
                await writer.WriteAsync(contents).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
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
    }
}