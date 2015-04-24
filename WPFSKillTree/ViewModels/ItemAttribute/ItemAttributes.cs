using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Raven.Json.Linq;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public class ItemAttributes :INotifyPropertyChanged
    {
        private readonly List<Attribute> aList = new List<Attribute>();
        private Dictionary<string, List<float>> AgregatedAttributes;
        public ListCollectionView Attributes;
        private List<Item> _Equip = new List<Item>();

        public List<Item> Equip
        {
            get { return _Equip; }
            set 
            { 
                _Equip = value;
                OnPropertyChanged("Equip");
            }
        }

        public List<Attribute> NonLocalMods = new List<Attribute>();

        public ItemAttributes(string itemData)
        {
            #region Readin

            RavenJObject jObject = RavenJObject.Parse(itemData);
            foreach (RavenJObject jobj in (RavenJArray) jObject["items"])
            {
                var html = jobj["x"].Value<string>();
                //html =
                //   html.Replace("\\\"", "\"").Replace("\\/", "/").Replace("\\n", " ").Replace("\\t", " ").Replace(
                //     "\\r", "").Replace("e\"", "e\" ").Replace("\"style", "\" style");
                var id = jobj["inventoryId"].Value<string>();
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
                foreach (var attr in item.Attributes)
                {
                    if (attr.Key == "Quality: #") continue;
                    aList.Add(new Attribute(attr.Key, attr.Value, item.Class.ToString()));
                }

                foreach (ItemMod mod in item.Mods)
                {
                    Attribute attTo = null;
                    attTo =
                        aList.Find(
                            ad =>
                                ad.TextAttribute == mod.Attribute &&
                                ad.Group == (mod.isLocal ? item.Class.ToString() : "Independent"));
                    if (attTo == null)
                    {
                        aList.Add(new Attribute(mod.Attribute, mod.Value,
                            (mod.isLocal ? item.Class.ToString() : "Independent")));
                    }
                    else
                    {
                        attTo.Add(mod.Attribute, mod.Value);
                    }
                }


                foreach (var attr in item.Attributes)
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

                foreach (ItemMod mod in item.Mods)
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


            var pgd = new PropertyGroupDescription("");
            pgd.PropertyName = "Group";
            Attributes.GroupDescriptions.Add(pgd);
            Attributes.CustomSort = new NumberLessStringComparer();


            var itemsBinding = new Binding();

            Attributes.Refresh();
        }

        private void AddItem(RavenJObject val, Item.ItemClass iclass)
        {
            Item item = null;
            item = new Item(iclass, val);
            Equip.Add(item);
        }

        public class Attribute : INotifyPropertyChanged
        {
            private readonly string _attribute;
            private readonly Regex _backreplace = new Regex("#");
            private readonly string _group;
            private readonly List<float> _value;

            public Attribute(string s, List<float> val, string grp)
            {
                _attribute = s;
                _value = new List<float>(val);
                _group = grp;
            }

            public List<float> Value
            {
                get { return _value; }
            }

            public string Group
            {
                get { return _group; }
            }

            public string TextAttribute
            {
                get { return _attribute; }
            }

            public string ValuedAttribute
            {
                get { return InsertNumbersInAttributes(_attribute, _value); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private string InsertNumbersInAttributes(string s, List<float> attrib)
            {
                foreach (float f in attrib)
                {
                    s = _backreplace.Replace(s, f + "", 1);
                }
                return s;
            }

            public bool Add(string s, List<float> val)
            {
                if (_attribute != s) return false;
                if (_value.Count != val.Count) return false;
                for (int i = 0; i < val.Count; i++)
                {
                    _value[i] += val[i];
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
        }

        public class NumberLessStringComparer : IComparer
        {
            private static readonly Regex numberfilter = new Regex("[0-9]*\\.?[0-9]+");

            public int Compare(object x, object y)
            {
                if (x is Attribute && y is Attribute)
                {
                    if (((Attribute) x).Group == "Independent" && !(((Attribute) y).Group == "Independent")) return +1;
                    if (((Attribute) y).Group == "Independent" && !(((Attribute) x).Group == "Independent")) return -1;
                    return
                        numberfilter.Replace(((Attribute) y).Group, "")
                            .CompareTo(numberfilter.Replace(((Attribute) y).Group, ""));
                }
                return 0;
            }

            public int Compare(string x, string y)
            {
                return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}