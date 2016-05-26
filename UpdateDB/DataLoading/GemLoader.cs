using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using POESKillTree.Model.Items.Enums;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using UpdateDB.DataLoading.Gems;

namespace UpdateDB.DataLoading
{
    // todo ItemDB and GamepediaReader do not really allow async gem loading
    public class GemLoader : DataLoader
    {
        private readonly IGemReader _gemReader;

        public override bool SavePathIsFolder
        {
            get { return false; }
        }

        public GemLoader(IGemReader gemReader)
        {
            _gemReader = gemReader;
        }

        protected override async Task LoadAsync(HttpClient httpClient)
        {
            var wikiUtils = new WikiUtils(httpClient);
            var gems = await wikiUtils.SelectFromGemsAsync(ParseGemTable);
            gems.Where(g => g != null)
                .GroupBy(g => g.Name).Select(g => g.First()) // distinct by Gem.Name
                .ForEach(ItemDB.Add);
        }

        private IEnumerable<ItemDB.Gem> ParseGemTable(HtmlNode table, ItemType itemType)
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