using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils.WikiApi;

using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves images of items (bases and uniques) from the Wiki through its API.
    /// </summary>
    public class ItemImageLoader : DataLoader
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImageLoader));

        // the wiki's item classes for which images are retrieved
        private static readonly IReadOnlyList<string> RelevantWikiClasses = new[]
        {
            "One Hand Axes", "Two Hand Axes", "Bows", "Claws", "Daggers",
            "One Hand Maces", "Sceptres", "Two Hand Maces", "Staves",
            "One Hand Swords", "Thrusting One Hand Swords", "Two Hand Swords", "Wands",
            "Amulets", "Belts", "Quivers", "Rings",
            "Body Armours", "Boots", "Helmets", "Gloves", "Shields", "Jewel",
            "Active Skill Gems", "Support Skill Gems",
        };

        public override bool SavePathIsFolder
        {
            get { return true; }
        }

        protected override async Task LoadAsync()
        {
            if (Directory.Exists(SavePath))
                Directory.Delete(SavePath, true);
            Directory.CreateDirectory(SavePath);

            // .ToList() so all tasks are started
            var tasks = RelevantWikiClasses.Select(ReadJson).ToList();
            await Task.WhenAll(tasks);
        }

        private async Task ReadJson(string wikiClass)
        {
            // for items that have the given class ...
            var conditions = new ConditionBuilder
            {
                {RdfItemClass, wikiClass}
            };
            // ... retrieve name and the icon url
            var task = WikiApiAccessor.AskAndQueryImageInforUrls(conditions);
            var results = (await task).ToList();

            // download the images from the urls and save them
            foreach (var result in results)
            {
                var data = await HttpClient.GetByteArrayAsync(result.Url);
                foreach (var name in result.Names)
                {
                    var fileName = name + ".png";
                    WikiApiUtils.SaveImage(data, Path.Combine(SavePath, fileName), true);
                }
            }

            Log.Info($"Retrieved {results.Count} images for class {wikiClass}.");
        }
    }
}