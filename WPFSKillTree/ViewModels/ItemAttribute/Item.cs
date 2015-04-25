using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Raven.Json.Linq;
using System.ComponentModel;
using System.Linq;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public enum FrameType
    {
        White = 0,
        Magic = 1,
        Rare = 2,
        Unique = 3,
        Gem = 4,
    }

    public class Item : INotifyPropertyChanged
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


        private FrameType _frame;
        public FrameType Frame
        {
            get { return _frame; }
            set { _frame = value; OnPropertyChanged("Frame"); }
        }
        List<ItemMod> _properties = new List<ItemMod>();
        public List<ItemMod> Properties
        {
            get { return _properties; }
            set { _properties = value; OnPropertyChanged("Properties"); OnPropertyChanged("HaveProperties"); }
        }

        List<ItemMod> _requirements = new List<ItemMod>();
        public List<ItemMod> Requirements
        {
            get { return _requirements; }
            set { _requirements = value; OnPropertyChanged("Requirements"); OnPropertyChanged("HaveRequirements"); }
        }

        List<ItemMod> _implicitMods = new List<ItemMod>();
        public List<ItemMod> ImplicitMods
        {
            get { return _implicitMods; }
            set { _implicitMods = value; OnPropertyChanged("Implicitmods"); OnPropertyChanged("HaveImplicitMods"); }
        }

        List<ItemMod> _explicitMods = new List<ItemMod>();
        public List<ItemMod> ExplicitMods
        {
            get { return _explicitMods; }
            set { _explicitMods = value; OnPropertyChanged("ExplicitMods"); OnPropertyChanged("HaveExplicitMods"); }
        }

        List<ItemMod> _craftedMods = new List<ItemMod>();
        public List<ItemMod> CraftedMods
        {
            get { return _craftedMods; }
            set { _craftedMods = value; OnPropertyChanged("Implicitmods"); OnPropertyChanged("HaveCraftedMods"); }
        }


        public bool HaveProperties
        {
            get { return _properties.Count > 0; }
        }

        public bool HaveRequirements
        {
            get { return _requirements.Count > 0; }
        }

        public bool HaveImplicitMods
        {
            get { return _implicitMods.Count > 0; }
        }

        public bool HaveExplicitMods
        {
            get { return _explicitMods.Count > 0; }
        }

        public bool HaveCraftedMods
        {
            get { return _craftedMods.Count > 0; }
        }

        string _flavourText = null;

        public string FlavourText
        {
            get { return _flavourText; }
            set { _flavourText = value; OnPropertyChanged("FlavourText"); OnPropertyChanged("HaveFlavourText"); }
        }

        public bool HaveFlavourText
        {
            get { return !string.IsNullOrEmpty(_flavourText); }
        }


        public List<ItemMod> Mods;

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("Name"); }
        }
        // The socket group of gem (all gems with same socket group value are linked).
        public int SocketGroup;
        private string _Type;

        public string Type
        {
            get { return _Type; }
            private set { _Type = value; OnPropertyChanged("Type"); }
        }


        public Item(ItemClass iClass, RavenJObject val)
        {
            Attributes = new Dictionary<string, List<float>>();
            Mods = new List<ItemMod>();
            Class = iClass;

            Name = val["name"].Value<string>();
            if (Name == "")
                Name = val["typeLine"].Value<string>();
            Type = val["typeLine"].Value<string>();

            Frame = (FrameType)val["frameType"].Value<int>();

            if (val.ContainsKey("properties"))
                foreach (RavenJObject obj in (RavenJArray)val["properties"])
                {
                    var values = new List<float>();
                    string s = "";

                    foreach (RavenJArray jva in (RavenJArray)obj["values"])
                    {
                        s += " " + jva[0].Value<string>();
                    }
                    s = s.TrimStart();

                    if (s == "")
                    {
                        Properties.Add(ItemMod.CreateMod(this, obj["name"].Value<string>(), numberfilter));

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

                    Properties.Add(ItemMod.CreateMod(this, obj["name"].Value<string>()+": "+s, numberfilter));
                    Attributes.Add(cs, values);
                }

            if (val.ContainsKey("requirements"))
            {
                string reqs = "";
                List<float> numbers = new List<float>();

                foreach (RavenJObject obj in (RavenJArray)val["requirements"])
                {
                    var n = obj["name"].Value<string>();

                    if (obj["displayMode"].Value<int>() == 0)
                        n = n + " #";
                    else
                        n = "# " + n;

                    numbers.Add(((RavenJArray)((RavenJArray)obj["values"])[0])[0].Value<float>());

                    if (!string.IsNullOrEmpty(reqs))
                        reqs += ", " + n;
                    else
                        reqs += n;
                }

                var m = ItemMod.CreateMod(this, "Requires " + reqs, numberfilter);
                m.Value = numbers;
                Requirements.Add(m);          
            }


            if (val.ContainsKey("implicitMods"))
                foreach (string s in val["implicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    ImplicitMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }
            if (val.ContainsKey("explicitMods"))
                foreach (string s in val["explicitMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    ExplicitMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }

            if (val.ContainsKey("craftedMods"))
                foreach (string s in val["craftedMods"].Values<string>())
                {
                    List<ItemMod> mods = ItemMod.CreateMods(this, s.Replace("Additional ", ""), numberfilter);
                    Mods.AddRange(mods);

                    CraftedMods.Add(ItemMod.CreateMod(this, s, numberfilter));
                }

            if (val.ContainsKey("flavourText"))
                FlavourText = string.Join("\r\n", val["flavourText"].Values<string>());


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

        // Returns gems linked to specified gem.
        public List<Item> GetLinkedGems(Item gem)
        {
            var link = new List<Item>();

            foreach (Item linked in Gems)
                if (linked != gem && linked.SocketGroup == gem.SocketGroup)
                    link.Add(linked);

            return link;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

    }
}