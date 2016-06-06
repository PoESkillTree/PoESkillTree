using System.IO;
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
    }
}