using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB.DataLoading
{
    // todo ItemDB and GamepediaReader do not really allow async gem loading
    /// <summary>
    /// Loads the available Gems from the unofficial Wiki at Gamepedia and loads and saves information about each
    /// gem using a <see cref="IGemReader"/>.
    /// </summary>
    public class GemLoader : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GemLoader));

        private readonly IGemReader _gemReader;

        public override bool SavePathIsFolder
        {
            get { return false; }
        }

        public override IEnumerable<string> SupportedArguments
        {
            get { return new []{ "single", "update", "merge" }; }
        }

        public GemLoader(IGemReader gemReader)
        {
            _gemReader = gemReader;
        }

        protected override Task LoadAsync(HttpClient httpClient)
        {
            string singleGemName;
            if (SuppliedArguments.TryGetValue("single", out singleGemName) && singleGemName != null)
            {
                return Task.Run(() => UpdateSingle(singleGemName));
            }
            string mergePath;
            if (SuppliedArguments.TryGetValue("merge", out mergePath) && mergePath != null)
            {
                return Task.Run(() => Merge(mergePath));
            }
            if (SuppliedArguments.ContainsKey("update"))
            {
                return Task.Run(() => Update());
            }
            return Overwrite(httpClient);
        }

        private void UpdateSingle(string singleGemName)
        {
            var fetched = _gemReader.FetchGem(singleGemName);
            if (fetched == null)
                return;
            if (!LoadOld())
                return;
            var old = ItemDB.GetGem(singleGemName);
            if (old == null)
                ItemDB.Add(fetched);
            else
                old.Merge(fetched);
        }

        private void Merge(string mergePath)
        {
            if (!LoadOld())
                return;
            ItemDB.MergeFromCompletePath(mergePath);
        }

        private void Update()
        {
            if (!LoadOld())
                return;
            foreach (var gem in ItemDB.GetAllGems())
            {
                var fetched = _gemReader.FetchGem(gem.Name);
                if (fetched != null)
                    gem.Merge(fetched);
            }
        }

        private bool LoadOld()
        {
            var updateSource = SavePath.Replace(".tmp", "");
            if (!File.Exists(updateSource))
            {
                Log.ErrorFormat("There is no gem file that can be updated (path: {0})", updateSource);
                return false;
            }
            ItemDB.LoadFromCompletePath(updateSource);
            return true;
        }

        private async Task Overwrite(HttpClient httpClient)
        {
            var wikiUtils = new WikiUtils(httpClient);
            var gems = await wikiUtils.SelectFromGemsAsync(ParseGemTable);
            gems.Where(g => g != null)
                .GroupBy(g => g.Name).Select(g => g.First()) // distinct by Gem.Name
                .ForEach(ItemDB.Add);
        }

        private IEnumerable<ItemDB.Gem> ParseGemTable(HtmlNode table)
        {
            return from row in table.Elements("tr").Skip(1)
                   select row.ChildNodes[0] into cell
                   select cell.SelectNodes("span/a")[1] into nameNode
                   select _gemReader.FetchGem(nameNode.InnerHtml);
        }

        protected override Task CompleteSavingAsync()
        {
            ItemDB.WriteToCompletePath(SavePath);
            return Task.WhenAll();
        }
    }
}