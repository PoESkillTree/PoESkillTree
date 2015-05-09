using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Xml.Linq;
using System.IO;
using POESKillTree.ViewModels.Items;
using MB.Algodat;

namespace POESKillTree.Utils
{
    internal class ItemAssetDownloader
    {

        static Dictionary<string, ItemClass> JewelryClassMap = new Dictionary<string, ItemClass>()
        {
            {"Amulet", ItemClass.Amulet},
            {"Belt",ItemClass.Belt},
            {"Ring", ItemClass.Ring},
        };

        static Dictionary<string, GearGroup> JewelryGroupMap = new Dictionary<string, GearGroup>()
        {
            {"Amulet", GearGroup.Amulet},
            {"Belt",GearGroup.Belt},
            {"Ring", GearGroup.Ring},
        };


        static Dictionary<string, ItemClass> WeaponClassMap = new Dictionary<string, ItemClass>()
        {
            {"Bow",ItemClass.TwoHand},
            {"Claw",ItemClass.OneHand},
            {"Dagger",ItemClass.OneHand},
            {"One Hand Axe",ItemClass.OneHand},
            {"One Hand Mace",ItemClass.OneHand},
            {"One Hand Sword",ItemClass.OneHand},
            {"Sceptre",ItemClass.OneHand},
            {"Staff",ItemClass.TwoHand},
            {"Thrusting One Hand Sword",ItemClass.OneHand},
            {"Two Hand Axe",ItemClass.TwoHand},
            {"Two Hand Mace",ItemClass.TwoHand},
            {"Two Hand Sword",ItemClass.TwoHand},
            {"Wand",ItemClass.OneHand},
        };

        static Dictionary<string, GearGroup> WeaponGroupMap = new Dictionary<string, GearGroup>()
        {
            {"Bow",GearGroup.Bow},
            {"Claw",GearGroup.Claw},
            {"Dagger",GearGroup.Dagger},
            {"One Hand Axe",GearGroup.Axe},
            {"One Hand Mace",GearGroup.Mace},
            {"One Hand Sword",GearGroup.Sword},
            {"Sceptre",GearGroup.Sceptre},
            {"Staff",GearGroup.Staff},
            {"Thrusting One Hand Sword",GearGroup.Sword},
            {"Two Hand Axe",GearGroup.Axe},
            {"Two Hand Mace",GearGroup.Mace},
            {"Two Hand Sword",GearGroup.Sword},
            {"Wand",GearGroup.Wand},
        };

        static Dictionary<string, ItemClass> ArmorClassMap = new Dictionary<string, ItemClass>()
        {
            {"Body Armour",ItemClass.Armor},
            {"Boots",ItemClass.Boots},
            {"Gloves",ItemClass.Gloves},
            {"Helmet",ItemClass.Helm},
            {"Shield",ItemClass.OffHand},
        };


        static Dictionary<string, GearGroup> ArmorGroupMap = new Dictionary<string, GearGroup>()
        {
            {"Body Armour",GearGroup.Chest},
            {"Boots",GearGroup.Boots},
            {"Gloves",GearGroup.Gloves},
            {"Helmet",GearGroup.Helmet},
            {"Shield",GearGroup.Shield},
        };




        public static void ExtractJewelry(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/jewelry", "GET");

            var breaks = doc.DocumentNode.SelectNodes("//br");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i++)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;

                    var name = namel[1].InnerText;
                    var level = namel[2].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var xe = new ItemBase()
                    {
                        GearGroup = JewelryGroupMap[h],
                        Class = JewelryClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level)

                    };

                    var implModnodes = new string[2][];
                    implModnodes[0] = namel[3].InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    implModnodes[1] = namel[4].InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries).ToArray();


                    if (implModnodes.Length > 0)
                    {
                        for (int j = 0; j < implModnodes[0].Length; j++)
                            xe.ImplicitMods.Add(new Stat(implModnodes[0][j], implModnodes[1][j]));
                    }


                    items.Add(xe);
                }
            }

        }

        public static void ExtractArmors(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/armour", "GET");

            var breaks = doc.DocumentNode.SelectNodes("//br");

            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i += 2)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                    var name = namel[1].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var level = namel[2].InnerText;


                    var armour = namel[3].InnerText;
                    var evasionrating = namel[4].InnerText;
                    var energyshield = namel[5].InnerText;

                    var reqStr = namel[6].InnerText;
                    var reqDex = namel[7].InnerText;
                    var reqInt = namel[8].InnerText;


                    var xe = new ItemBase()
                    {
                        GearGroup = ArmorGroupMap[h],
                        Class = ArmorClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level),
                        RequiredStr = int.Parse(reqStr),
                        RequiredDex = int.Parse(reqDex),
                        RequiredInt = int.Parse(reqInt),
                    };


                    if (armour != "0")
                        xe.Properties.Add(new Stat("Armour", armour ));
                    if (evasionrating != "0")
                        xe.Properties.Add(new Stat( "Energy Shield", evasionrating ));
                    if (energyshield != "0")
                        xe.Properties.Add(new Stat( "Evasion Rating", energyshield ));


                    var implModnodes = lines[i + 1].SelectNodes("td")
                        .Select(n => n.InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(v => v.Any(s => !string.IsNullOrEmpty(s))).ToArray();

                    if (implModnodes.Length > 0)
                    {
                        implModnodes[0] = implModnodes[0].Select(n => n.Replace("Minimum", "").Replace("Maximum", "").Trim()).Distinct().ToArray();

                        for (int j = 0; j < implModnodes[0].Length; j++)
                        {
                            xe.ImplicitMods.Add(new Stat(implModnodes[0][j],implModnodes[1][j]));
                        }
                    }


                    items.Add(xe);
                }
            }
        }

        public static void ExtractWeapons(List<ItemBase> items, List<Tuple<string, string>> images)
        {
            var web = new HtmlWeb();

            var doc = web.Load(@"http://www.pathofexile.com/item-data/weapon", "GET");


            var itemTypeHeader = doc.DocumentNode.SelectNodes(@"//h1[contains(@class,'topBar') and contains(@class,'last') and contains(@class,'layoutBoxTitle')]");


            XElement root = new XElement("itembaselist");

            foreach (var header in itemTypeHeader)
            {
                var h = header.InnerText;
                var table = header.SelectSingleNode("following::table[@class='itemDataTable']");
                var lines = table.SelectNodes("tr[not(descendant-or-self::th)]");

                for (int i = 0; i < lines.Count; i += 2)
                {
                    var namel = lines[i].SelectNodes("td");

                    var image = namel[0].SelectSingleNode("img[@data-large-image]").Attributes["data-large-image"].Value;
                    var name = namel[1].InnerText;

                    images.Add(Tuple.Create(name, image));

                    var level = namel[2].InnerText;
                    var damage = namel[3].InnerText;
                    var aps = namel[4].InnerText;
                    var dps = namel[5].InnerText;
                    var reqStr = namel[6].InnerText;
                    var reqDex = namel[7].InnerText;
                    var reqInt = namel[8].InnerText;

                    var xe = new ItemBase()
                    {
                        GearGroup = WeaponGroupMap[h],
                        Class = WeaponClassMap[h],
                        ItemType = name,
                        Level = int.Parse(level),
                        RequiredStr = int.Parse(reqStr),
                        RequiredDex = int.Parse(reqDex),
                        RequiredInt = int.Parse(reqInt),
                    };



                    xe.Properties.Add(new Stat("Physical Damage", damage));

                    xe.Properties.Add(new Stat("Atacks Per Second", aps ));

                    xe.Properties.Add(new Stat("Critical Strike Chance %", "5"));


                    var implModnodes = lines[i + 1].SelectNodes("td")
                        .Select(n => n.InnerHtml.Trim().Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
                        .Where(v => v.Any(s => !string.IsNullOrEmpty(s))).ToArray();

                    if (implModnodes.Length > 0)
                    {
                        implModnodes[0] = implModnodes[0].Select(n => n.Replace("Minimum", "").Replace("Maximum", "").Trim()).Distinct().ToArray();

                        for (int j = 0; j < implModnodes[0].Length; j++)
                        {
                            xe.ImplicitMods.Add(new Stat(implModnodes[0][j], implModnodes[1][j]));
                        }
                    }

                    items.Add(xe);
                }
            }
        }
    }
}
