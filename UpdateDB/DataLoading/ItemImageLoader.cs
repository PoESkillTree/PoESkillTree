using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using POESKillTree.Utils.WikiApi;

using static POESKillTree.Utils.WikiApi.WikiApiUtils;
using static POESKillTree.Utils.WikiApi.ItemRdfPredicates;

namespace UpdateDB.DataLoading
{
    /// <summary>
    /// Retrieves images of item bases from the unofficial Wiki through its API.
    /// </summary>
    public class ItemImageLoader : MultiDataLoader<Task<byte[]>>
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(ItemImageLoader));

        // the wiki's item classes for which images are retrieved
        private static readonly IReadOnlyList<string> RelevantWikiClasses = new[]
        {
            "One Hand Axes", "Two Hand Axes", "Bows", "Claws", "Daggers",
            "One Hand Maces", "Sceptres", "Two Hand Maces", "Staves",
            "One Hand Swords", "Thrusting One Hand Swords", "Two Hand Swords", "Wands",
            "Amulets", "Belts", "Quivers", "Rings",
            "Body Armours", "Boots", "Helmets", "Shields", "Jewel",
        };

        private readonly bool _overwriteExisting;

        /// <param name="overwriteExisting">if false, only images whose files don't exist are downloaded</param>
        public ItemImageLoader(bool overwriteExisting)
        {
            _overwriteExisting = overwriteExisting;
        }

        protected override async Task LoadAsync()
        {
            if (Directory.Exists(SavePath))
                Directory.Delete(SavePath, true);
            Directory.CreateDirectory(SavePath);

            await Task.WhenAll(RelevantWikiClasses.Select(ReadJson));
        }

        private async Task ReadJson(string wikiClass)
        {
            // for items with Normal rarity that can drop and have the given class ...
            var conditions = new ConditionBuilder
            {
                {RdfRarity, "Normal"},
                {RdfDropEnabled, "true"},
                {RdfItemClass, wikiClass}
            };
            // ... retrieve name and the icon page
            var printouts = new[] {RdfName, RdfIcon};
            // the icon page entry is an array, the page title is the "fulltext" property in its first element
            var results = (from result in await WikiApiAccessor.AskArgs(conditions, printouts)
                           let title = result[RdfIcon].First.Value<string>("fulltext")
                           let name = SingularValue<string>(result, RdfName)
                           select new {name, title}).ToList();

            // query the image url in each icon page and save them
            var titleToName = results.ToDictionary(x => x.title, x => x.name);
            foreach (var tuple in await WikiApiAccessor.QueryImageInfoUrls(results.Select(t => t.title)))
            {
                SaveImage(titleToName[tuple.Item1] + ".png", tuple.Item2);
            }

            Log.Info($"Retrieved {results.Count} images for class {wikiClass}.");
        }

        private void SaveImage(string fileName, string url)
        {
            if (_overwriteExisting || !File.Exists(Path.Combine(SavePath, fileName)))
                AddSaveTask(fileName, HttpClient.GetByteArrayAsync(url));
        }

        protected override async Task SaveDataToStreamAsync(Task<byte[]> data, Stream stream)
        {
            WikiApiUtils.SaveImage(await data, stream, true);
        }
    }
}