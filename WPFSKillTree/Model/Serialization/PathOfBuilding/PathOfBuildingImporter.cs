using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    public class PathOfBuildingImporter
    {
        private readonly HttpClient _httpClient;

        public PathOfBuildingImporter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IBuild?> FromPastebinAsync(string pastebinUrl)
        {
            var url = pastebinUrl.Replace("pastebin.com/", "pastebin.com/raw/");
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var base64 = await response.Content.ReadAsStringAsync();
            return await FromBase64Async(base64);
        }

        public async Task<IBuild?> FromBase64Async(string input)
        {
            var compressed = Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/'));
            await using var ms = new MemoryStream(compressed);
            // Skip compression type header
            ms.Seek(2, SeekOrigin.Begin);
            await using var deflateStream = new DeflateStream(ms, CompressionMode.Decompress);
            using var deflateReader = new StreamReader(deflateStream);
            var decompressed = await deflateReader.ReadToEndAsync();

            Debug.WriteLine(decompressed);
            return null;
        }
    }
}