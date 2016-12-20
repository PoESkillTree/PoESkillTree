using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using log4net;
using POESKillTree.Model.Gems;
using POESKillTree.Utils;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB.DataLoading
{
    // todo ItemDB does not really allow async gem loading
    /// <summary>
    /// Loads the available Gems from the unofficial Wiki at Gamepedia and loads and saves information about each
    /// gem using a <see cref="IGemReader"/>.
    /// </summary>
    public class GemLoader : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GemLoader));

        private readonly IGemReader _gemReader;
        private readonly GemDB DB;

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
            DB = new GemDB();
        }

        protected override Task LoadAsync(HttpClient httpClient)
        {
            _gemReader.HttpClient = httpClient;
            string singleGemName;
            if (SuppliedArguments.TryGetValue("single", out singleGemName) && singleGemName != null)
            {
                return UpdateSingle(singleGemName);
            }
            string mergePath;
            if (SuppliedArguments.TryGetValue("merge", out mergePath) && mergePath != null)
            {
                return Task.Run(() => Merge(mergePath));
            }
            if (SuppliedArguments.ContainsKey("update"))
            {
                return Update();
            }
            return Overwrite(httpClient);
        }

        private async Task UpdateSingle(string singleGemName)
        {
            var fetched = await _gemReader.FetchGemAsync(singleGemName);
            if (fetched == null)
                return;
            if (!LoadOld())
                return;
            var old = DB.GetGem(singleGemName);
            if (old == null)
                DB.Add(fetched);
            else
                old.Merge(fetched);
        }

        private void Merge(string mergePath)
        {
            if (!LoadOld())
                return;
            DB.Merge(GemDB.LoadFromText(File.ReadAllText(mergePath)));
        }

        private async Task Update()
        {
            if (!LoadOld())
                return;
            foreach (var gem in DB.Gems)
            {
                var fetched = await _gemReader.FetchGemAsync(gem.Name);
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
            GemDB.LoadFromText(File.ReadAllText(updateSource));
            return true;
        }

        private async Task Overwrite(HttpClient httpClient)
        {
            var wikiUtils = new WikiUtils(httpClient);
            var gemTasks = await wikiUtils.SelectFromGemsAsync(ParseGemTable);
            foreach (var task in gemTasks)
            {
                var gem = await task;
                if (gem == null)
                    continue;
                DB.Add(gem);
            }
        }

        private IEnumerable<Task<Gem>> ParseGemTable(HtmlNode table)
        {
            return from row in table.Elements("tr").Skip(1)
                   select row.ChildNodes[0] into cell
                   select cell.SelectNodes("span/a[not(contains(@class, 'image'))]")[0] into nameNode
                   select _gemReader.FetchGemAsync(nameNode.InnerHtml);
        }

        protected override Task CompleteSavingAsync()
        {
            DB.WriteToCompletePath(SavePath);
            return Task.WhenAll();
        }
    }
}