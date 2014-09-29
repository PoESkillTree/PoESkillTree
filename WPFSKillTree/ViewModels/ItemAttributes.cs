using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Xml;
using Raven.Json.Linq;
using Damage = POESKillTree.SkillTreeFiles.Compute.Damage;

namespace POESKillTree.ViewModels
{
    public class ItemAttributes
    {
        public ItemAttributes(string itemData)
        {
            #region Readin
            RavenJObject jObject = RavenJObject.Parse(itemData);
            foreach (RavenJObject jobj in (RavenJArray)jObject["items"])
            {
                string html = jobj["x"].Value<string>();
                //html =
                //   html.Replace("\\\"", "\"").Replace("\\/", "/").Replace("\\n", " ").Replace("\\t", " ").Replace(
                //     "\\r", "").Replace("e\"", "e\" ").Replace("\"style", "\" style");
                string id = jobj["inventoryId"].Value<string>();
                if (id == "BodyArmour")
                {
                    AddItem(jobj, Item.ItemClass.Armor);
                }
                if (id == "Ring" || id == "Ring2")
                {
                    AddItem(jobj, Item.ItemClass.Ring);
                }
                if (id == "Gloves")
                {
                    AddItem(jobj, Item.ItemClass.Gloves);
                }
                if (id == "Weapon")
                {
                    AddItem(jobj, Item.ItemClass.MainHand);
                }
                if (id == "Offhand")
                {
                    AddItem(jobj, Item.ItemClass.OffHand);
                }
                if (id == "Helm")
                {
                    AddItem(jobj, Item.ItemClass.Helm);
                }
                if (id == "Boots")
                {
                    AddItem(jobj, Item.ItemClass.Boots);
                }
                if (id == "Amulet")
                {
                    AddItem(jobj, Item.ItemClass.Amulet);
                }
                if (id == "Belt")
                {
                    AddItem(jobj, Item.ItemClass.Belt);
                }



            }
            #endregion
            aList.Clear();
            NonLocalMods.Clear();
            Attributes = new ListCollectionView(aList);
            foreach (Item item in Equip)
            {
                foreach (KeyValuePair<string, List<float>> attr in item.Attributes)
                {
                    if (attr.Key == "Quality: #") continue;
                    aList.Add(new Attribute(attr.Key, attr.Value, item.Class.ToString()));
                }

                foreach (Item.Mod mod in item.Mods)
                {
                    Attribute attTo = null;
                    attTo = aList.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == (mod.isLocal ? item.Class.ToString() : "Independent"));
                    if (attTo == null)
                    {
                        aList.Add(new Attribute(mod.Attribute, mod.Value, (mod.isLocal ? item.Class.ToString() : "Independent")));
                    }
                    else
                    {
                        attTo.Add(mod.Attribute, mod.Value);
                    }

                }


                foreach (KeyValuePair<string, List<float>> attr in item.Attributes)
                {
                    if (attr.Key == "Quality: +#%") continue;
                    if (attr.Key == "Attacks per Second: #") continue;
                    if (attr.Key == "Critical Strike Chance: #%") continue;
                    if (attr.Key.ToLower().Contains("damage")) continue;
                    if (attr.Key.Contains("Weapon Class")) continue;
                    if (attr.Key.Contains("Elemental Damage")) continue;
                    Attribute attTo = null;
                    attTo = NonLocalMods.Find(ad => ad.TextAttribute == attr.Key);
                    if (attTo == null)
                    {
                        NonLocalMods.Add(new Attribute(attr.Key, attr.Value, ""));
                    }
                    else
                    {
                        attTo.Add(attr.Key, attr.Value);
                    }
                }

                foreach (Item.Mod mod in item.Mods)
                {
                    if (mod.isLocal) continue;
                    Attribute attTo = null;
                    attTo = NonLocalMods.Find(ad => ad.TextAttribute == mod.Attribute);
                    if (attTo == null)
                    {
                        NonLocalMods.Add(new Attribute(mod.Attribute, mod.Value, ""));
                    }
                    else
                    {
                        attTo.Add(mod.Attribute, mod.Value);
                    }

                }

            }



            PropertyGroupDescription pgd = new PropertyGroupDescription("");
            pgd.PropertyName = "Group";
            Attributes.GroupDescriptions.Add(pgd);
            Attributes.CustomSort = new NumberLessStringComparer();


            Binding itemsBinding = new Binding();

            Attributes.Refresh();

        }

        public class Item
        {
            public enum GemColorClass
            {
                Str, Int, Dex, White
            }

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

            public class Mod
            {
                enum ValueType
                {
                    Flat, Percentage, FlatMinMax
                }
                public static List<Mod> CreateMods(Item item, string attribute)
                {
                    ItemClass ic = item.Class;
                    List<Mod> mods = new List<Mod>();
                    List<float> values = new List<float>();
                    foreach (Match match in numberfilter.Matches(attribute))
                    {
                        values.Add(float.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture));
                    }
                    string at = numberfilter.Replace(attribute, "#");
                    if (at == "+# to all Attributes")
                    {
                        mods.Add(new Mod()
                        {
                            itemclass = ic,
                            Value = values,
                            Attribute = "+# to Strength"
                        });
                        mods.Add(new Mod()
                        {
                            itemclass = ic,
                            Value = values,
                            Attribute = "+# to Dexterity"
                        });
                        mods.Add(new Mod()
                        {
                            itemclass = ic,
                            Value = values,
                            Attribute = "+# to Intelligence"
                        });
                    }        
                    else
                    {
                        mods.Add(new Mod()
                        {
                            itemclass = ic,
                            Value = values,
                            Attribute = at,
                            isLocal = DetermineLocal(item, at)
                        });
                    }
                    return mods;
                }

                private ItemClass itemclass;
                public string Attribute;
                public List<float> Value;
                public bool isLocal = false;

                // Returns true if property/mod is local, false otherwise.
                static bool DetermineLocal(Item item, string attr)
                {
                    return (item.Class != Item.ItemClass.Amulet && item.Class != Item.ItemClass.Ring && item.Class != Item.ItemClass.Belt)
                           && (attr.Contains("Armour")
                               || attr.Contains("Evasion")
                               || attr.Contains("Energy Shield")
                               || attr.Contains("Weapon Class")
                               || attr.Contains("Critical Strike Chance with this Weapon")
                               || attr.Contains("Critical Strike Damage Multiplier with this Weapon"))
                           || (item.Class == Item.ItemClass.MainHand || item.Class == Item.ItemClass.OffHand)
                              && item.Keywords != null // Only weapons have keyword.
                              && (attr == "#% increased Attack Speed"
                                  || attr == "#% increased Accuracy Rating"
                                  || attr == "+# to Accuracy Rating"
                                  || attr.StartsWith("Adds ") && (attr.EndsWith(" Damage") || attr.EndsWith(" Damage in Main Hand") || attr.EndsWith(" Damage in Off Hand"))
                                  || attr == "#% increased Physical Damage");
                }
            }

            public ItemClass Class;
            public string Type;
            public string Name;
            public Dictionary<string, List<float>> Attributes;
            public List<Mod> Mods;
            public List<Item> Gems;
            public List<string> Keywords;
            public GemColorClass GemClass;
            // The socket group of gem (all gems with same socket group value are linked).
            public int SocketGroup;

            public Item(ItemClass iClass, RavenJObject val)
            {
                Attributes = new Dictionary<string, List<float>>();
                Mods = new List<Mod>();
                Class = iClass;
                if (iClass == ItemClass.Gem)
                {
                    switch (val["colour"].Value<string>())
                    {
                        case "S":
                            GemClass = GemColorClass.Str;
                            break;

                        case "D":
                            GemClass = GemColorClass.Dex;
                            break;

                        case "I":
                            GemClass = GemColorClass.Int;
                            break;

                        default:
                            GemClass = GemColorClass.White;
                            break;
                    }
                }
                else
                {
                    Gems = new List<Item>();
                }

                Name = val["name"].Value<string>();
                if (Name == "")
                    Name = val["typeLine"].Value<string>();
                Type = val["typeLine"].Value<string>();

                if (val.ContainsKey("properties"))
                    foreach (RavenJObject obj in (RavenJArray)val["properties"])
                    {
                        List<float> values = new List<float>();
                        string s = "";
                      
                        foreach (RavenJArray jva in (RavenJArray)obj["values"])
                        {
                            s += " "+jva[0].Value<string>() ;
                        }

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
                            else values.Add(float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        string cs = obj["name"].Value<string>() + ": " + (numberfilter.Replace(s, "#"));

                        Attributes.Add(cs, values);
                    }
                if (val.ContainsKey("explicitMods"))
                    foreach (string s in val["explicitMods"].Values<string>())
                    {
                        var mods = Mod.CreateMods(this, s.Replace("Additional ", ""));
                        Mods.AddRange(mods);
                    }
                if (val.ContainsKey("implicitMods"))
                    foreach (string s in val["implicitMods"].Values<string>())
                    {
                        var mods = Mod.CreateMods(this, s.Replace("Additional ", ""));
                        Mods.AddRange(mods);
                    }
                if (val.ContainsKey("craftedMods"))
                    foreach (string s in val["craftedMods"].Values<string>())
                    {
                        var mods = Mod.CreateMods(this, s.Replace("Additional ", ""));
                        Mods.AddRange(mods);
                    }

                List<int> Sockets = new List<int>();
                if (val.ContainsKey("sockets"))
                    foreach (RavenJObject obj in (RavenJArray)val["sockets"])
                    {
                        Sockets.Add(obj["group"].Value<int>());
                    }
                if (val.ContainsKey("socketedItems"))
                {
                    int socket = 0;
                    foreach (RavenJObject obj in (RavenJArray)val["socketedItems"])
                    {
                        var item = new Item(ItemClass.Gem, obj);
                        item.SocketGroup = Sockets[socket++];
                        Gems.Add(item);
                    }
                }
            }
            static Regex colorcleaner = new Regex("\\<.+?\\>");
            static Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");

            // Returns gems linked to specified gem.
            public List<Item> GetLinkedGems(Item gem)
            {
                List<Item> link = new List<Item>();

                foreach (Item linked in Gems)
                    if (linked != gem && linked.SocketGroup == gem.SocketGroup)
                        link.Add(linked);

                return link;
            }
        }

        public List<Item> Equip = new List<Item>();

        private Dictionary<string, List<float>> AgregatedAttributes;
        public ListCollectionView Attributes;
        private List<Attribute> aList = new List<Attribute>();
        public List<Attribute> NonLocalMods = new List<Attribute>();
        public class Attribute : INotifyPropertyChanged
        {
            public Attribute(string s, List<float> val, string grp)
            {
                attribute = s;
                value = new List<float>(val);
                group = grp;
            }

            Regex backreplace = new Regex("#");
            private string InsertNumbersInAttributes(string s, List<float> attrib)
            {
                foreach (var f in attrib)
                {
                    s = backreplace.Replace(s, f + "", 1);
                }
                return s;
            }
            private string attribute;
            private List<float> value;
            private string group;
            public List<float> Value { get { return value; } }
            public string TextAttribute
            {
                get { return attribute; }
            }

            public string ValuedAttribute
            {
                get { return InsertNumbersInAttributes(attribute, value); }
            }
            public string Group { get { return group; } }
            public bool Add(string s, List<float> val)
            {
                if (attribute != s) return false;
                if (value.Count != val.Count) return false;
                for (int i = 0; i < val.Count; i++)
                {
                    value[i] += val[i];
                }
                OnPropertyChanged("ValuedAttribute");
                return true;
            }

            private void OnPropertyChanged(string info)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(info));
                }
            }

            /*    public override string ToString()
                {
                    return ValuedAttribute;
                }*/
            public event PropertyChangedEventHandler PropertyChanged;
        }
        private void AddItem(RavenJObject val, Item.ItemClass iclass)
        {

            Item item = null;

            item = new Item(iclass, val);




            Equip.Add(item);
        }

        public class NumberLessStringComparer : System.Collections.IComparer
        {
            static Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");

            public int Compare(string x, string y)
            {
                return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
            }

            public int Compare(object x, object y)
            {
                if (x is Attribute && y is Attribute)
                {
                    if (((Attribute)x).Group == "Independent" && !(((Attribute)y).Group == "Independent")) return +1;
                    if (((Attribute)y).Group == "Independent" && !(((Attribute)x).Group == "Independent")) return -1;
                    return numberfilter.Replace(((Attribute)y).Group, "").CompareTo(numberfilter.Replace(((Attribute)y).Group, ""));
                }
                return 0;
            }
        }
    }
}
