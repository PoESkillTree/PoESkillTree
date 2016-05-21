using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace POESKillTree.Utils
{
    internal class ItemAssetDownloader
    {
        // todo

        public static void ExtractJewelry(List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/jewelry", "GET");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                foreach (var t in lines)
                {
                    try
                    {
                        var namel = t.SelectNodes("td");
                        var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                        var name = namel[1].InnerText;

                        images.Add(Tuple.Create(name, image));
                    }
                    catch(Exception e)
                    {
                    }
                }
            }

        }

        public static void ExtractArmors(List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/armour", "GET");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (var i = 0; i < lines.Count; i += 2)
                {
                    try
                    {
                        var namel = lines[i].SelectNodes("td");
                        var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                        var name = namel[1].InnerText;

                        images.Add(Tuple.Create(name, image));
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }

        public static void ExtractWeapons(List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/weapon", "GET");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i += 2)
                {
                    try
                    {
                        var namel = lines[i].SelectNodes("td");
                        var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                        var name = namel[1].InnerText;

                        images.Add(Tuple.Create(name, image));
                    }
                    catch(Exception e)
                    {

                    }
                }
            }
        }

        public static void ExtractJewels(List<Tuple<string, string>> images)
        {
            try
            {
                var crimson_jewel = "Crimson Jewel";
                var cobalt_jewel = "Cobalt Jewel";
                var viridian_jewel = "Viridian Jewel";
                images.Add(Tuple.Create(crimson_jewel, "https://p7p4m6s5.ssl.hwcdn.net/image/Art/2DItems/Jewels/basicstr.png?scale=1&w=1&h=1&v=5496129c557831c118a679c1001f3df93"));
                images.Add(Tuple.Create(cobalt_jewel, "https://p7p4m6s5.ssl.hwcdn.net/image/Art/2DItems/Jewels/basicint.png?scale=1&w=1&h=1&v=cd579ea22c05f1c6ad2fd015d7a710bd3"));
                images.Add(Tuple.Create(viridian_jewel, "https://p7p4m6s5.ssl.hwcdn.net/image/Art/2DItems/Jewels/basicdex.png?scale=1&w=1&h=1&v=7375b3bb90a9809870b31d1aa4aa68b93"));
            }
            catch (Exception e)
            {

            }
        }
    }
}
