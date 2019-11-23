using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Engine.Utils.WikiApi;
using PoESkillTree.Utils.WikiApi;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves images of items (bases and uniques) from the Wiki through its API.
    /// </summary>
    public class ItemImageLoader : IDataLoader
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        // the wiki's item classes for which images are retrieved
        private static readonly IReadOnlyList<string> RelevantWikiClasses = new[]
        {
            "One Hand Axes", "Two Hand Axes", "Bows", "Claws", "Daggers", "Rune Dagger",
            "One Hand Maces", "Sceptres", "Two Hand Maces", "Staves", "Warstaff",
            "One Hand Swords", "Thrusting One Hand Swords", "Two Hand Swords", "Wands",
            "Amulets", "Belts", "Quivers", "Rings",
            "Body Armours", "Boots", "Helmets", "Gloves", "Shields", "Jewel",
            "Active Skill Gems", "Support Skill Gems",
            "Life Flasks", "Mana Flasks", "Hybrid Flasks", "Utility Flasks", "Critical Utility Flasks",
        };

        public bool SavePathIsFolder => true;

        public async Task LoadAndSaveAsync(HttpClient httpClient, string savePath)
        {
            if (Directory.Exists(savePath))
                Directory.Delete(savePath, true);
            Directory.CreateDirectory(savePath);

            var apiAccessor = new ApiAccessor(httpClient);
            // .ToList() so all tasks are started
            var tasks = RelevantWikiClasses.Select(s => ReadJson(s, httpClient, apiAccessor, savePath)).ToList();
            await Task.WhenAll(tasks);
        }

        private static async Task ReadJson(string wikiClass, HttpClient httpClient, ApiAccessor apiAccessor, string savePath)
        {
            // for items that have the given class ...
            var where = $"{CargoConstants.ItemClass}='{wikiClass}'";
            // ... retrieve name and the icon url
            var task = apiAccessor.GetItemImageInfosAsync(where);
            var results = (await task).ToList();

            // download the images from the urls and save them
            foreach (var result in results)
            {
                var data = await httpClient.GetByteArrayAsync(result.Url);
                foreach (var name in result.Names)
                {
                    var fileName = name + ".png";
                    WikiApiUtils.SaveImage(data, Path.Combine(savePath, fileName), true);
                }
            }

            Log.Info($"Retrieved {results.Count} images for class {wikiClass}.");
        }
    }
}