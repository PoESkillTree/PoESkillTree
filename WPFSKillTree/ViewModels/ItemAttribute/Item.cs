using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Raven.Json.Linq;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public class Item
    {
        public enum ItemClass
        {
            Armor,
            MainHand,
            OffHand,
            Ring,
            Amulet,
            Helm,
            Gloves,
            Boots,
            Gem,
            Belt
        }

        private static Regex colorcleaner = new Regex("\\<.+?\\>");
        private static readonly Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");

        public Dictionary<string, List<float>> Attributes;
        public ItemClass Class;
        public List<Item> Gems;
        public List<string> Keywords;
        public List<ItemMod> Mods;
        public string Name;
        // The socket group of gem (all gems with same socket group value are linked).
        public int SocketGroup;
        public string Type;

        public Item(ItemClass iClass, RavenJObject val)
        {
            Attributes = new Dictionary<string, List<float>>();
            Mods = new List<ItemMod>();
            Class = iClass;

            Name = val["name"].Value<string>();
            if (Name == "")
                Name = val["typeLine"].Value<string>();
            Type = val["typeLine"].Value<string>();

            if (val.ContainsKey("properties"))
                foreach (RavenJObject obj in (RavenJArray) val["properties"])
                {
                    var values = new List<float>();
                    string s = "";

                    foreach (RavenJArray jva in (RavenJArray) obj["values"])
                    {
                        s += " " + jva[0].Value<string>();
                    }
                    s = s.TrimStart();

                    if (s == "")
                    {
                        Keywords = new List<string>();
                        string[] sl = obj["name"].Value<string>().Split(',');
                        foreach (string i in sl)
                            Keywords.Add(i.Trim());
                        continue;
                    }

                    foreach (Match m in numberfilter.Matches(s))
                    {
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                    }
                    string cs = obj["name"].Value<string>() + ": " + (numberfilter.Replace(s, "#"));

                    Attributes.Add(cs, values);
                }
            if (val.ContainsKey("explicitMods"))
                foreach (string s in val["explicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);
                }
            if (val.ContainsKey("implicitMods"))
                foreach (string s in val["implicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);
                }
            if (val.ContainsKey("craftedMods"))
                foreach (string s in val["craftedMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);
                }

            if (iClass == ItemClass.Gem)
            {
                switch (val["colour"].Value<string>())
                {
                    case "S":
                        Keywords.Add("Strength");
                        break;

                    case "D":
                        Keywords.Add("Dexterity");
                        break;

                    case "I":
                        Keywords.Add("Intelligence");
                        break;
                }
            }
            else
            {
                Gems = new List<Item>();
            }

            var Sockets = new List<int>();
            if (val.ContainsKey("sockets"))
                foreach (RavenJObject obj in (RavenJArray) val["sockets"])
                {
                    Sockets.Add(obj["group"].Value<int>());
                }
            if (val.ContainsKey("socketedItems"))
            {
                int socket = 0;
                foreach (RavenJObject obj in (RavenJArray) val["socketedItems"])
                {
                    var item = new Item(ItemClass.Gem, obj);
                    item.SocketGroup = Sockets[socket++];
                    Gems.Add(item);
                }
            }
        }

        // Returns gems linked to specified gem.
        public List<Item> GetLinkedGems(Item gem)
        {
            var link = new List<Item>();

            foreach (Item linked in Gems)
                if (linked != gem && linked.SocketGroup == gem.SocketGroup)
                    link.Add(linked);

            return link;
        }
    }
}