using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Linq;
using MahApps.Metro.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Items
{
    public class ItemAttributes : INotifyPropertyChanged
    {
        #region slotted items
        public Item Armor
        {
            get
            {
                return GetItemInSlot(ItemSlot.Armor);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Armor);
            }
        }
        public Item MainHand
        {
            get
            {
                return GetItemInSlot(ItemSlot.MainHand);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.MainHand);
            }
        }
        public Item OffHand
        {
            get
            {
                return GetItemInSlot(ItemSlot.OffHand);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.OffHand);
            }
        }
        public Item Ring
        {
            get
            {
                return GetItemInSlot(ItemSlot.Ring);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Ring);
            }
        }
        public Item Ring2
        {
            get
            {
                return GetItemInSlot(ItemSlot.Ring2);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Ring2);
            }
        }
        public Item Amulet
        {
            get
            {
                return GetItemInSlot(ItemSlot.Amulet);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Amulet);
            }
        }
        public Item Helm
        {
            get
            {
                return GetItemInSlot(ItemSlot.Helm);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Helm);
            }
        }
        public Item Gloves
        {
            get
            {
                return GetItemInSlot(ItemSlot.Gloves);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Gloves);
            }
        }
        public Item Boots
        {
            get
            {
                return GetItemInSlot(ItemSlot.Boots);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Boots);
            }
        }
        public Item Gem
        {
            get
            {
                return GetItemInSlot(ItemSlot.Gem);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Gem);
            }
        }
        public Item Belt
        {
            get
            {
                return GetItemInSlot(ItemSlot.Belt);
            }
            set
            {
                SetItemInSlot(value, ItemSlot.Belt);
            }
        }


        public Item GetItemInSlot(ItemSlot slot)
        {
            return _Equip.FirstOrDefault(i => i.Slot == slot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="slot"></param>
        /// <returns>true if item was set</returns>
        public bool SetItemInSlot(Item value, ItemSlot slot)
        {
            if (value != null)
            {
                if (value.Class == ItemClass.Unequipable)
                    return false;

                if ((int)value.Class != (int)slot && (slot == ItemSlot.Ring2 && value.Class != ItemClass.Ring))
                    return false;

            }
            if (slot == ItemSlot.Unequipable)
                return false;
            RemoveItemFromSlot(slot);

            if (value != null)
            {
                value.Slot = slot;
                _Equip.Add(value);
            }
            OnPropertyChanged(slot.ToString());
            RefreshItemAttributes();
            return true;
        }

        public Item RemoveItemFromSlot(ItemSlot slot)
        {
            var itm = _Equip.FirstOrDefault(i => i.Slot == slot);
            if (itm != null)
                _Equip.Remove(itm);

            OnPropertyChanged("Equip");
            OnPropertyChanged(slot.ToString());

            if (itm != null)
                itm.Slot = ItemSlot.Unequipable;
            return itm;
        }
        #endregion

        private readonly List<Attribute> aList = new List<Attribute>();
        private Dictionary<string, List<float>> AgregatedAttributes;

        private ListCollectionView _Attributes;

        private ObservableCollection<Item> _Equip = new ObservableCollection<Item>();

        public ObservableCollection<Item> Equip
        {
            get { return _Equip; }
            set
            {
                _Equip = value;
                OnPropertyChanged("Equip");
            }
        }

        public ListCollectionView Attributes
        {
            get
            {
                return _Attributes;
            }

            set
            {
                _Attributes = value;
                OnPropertyChanged("Attributes");
            }
        }

        public List<Attribute> NonLocalMods = new List<Attribute>();

        public ItemAttributes(string itemData)
        {
            #region Readin

            JObject jObject = JObject.Parse(itemData);
            foreach (JObject jobj in (JArray)jObject["items"])
            {
                var html = jobj["x"].Value<string>();
                //html =
                //   html.Replace("\\\"", "\"").Replace("\\/", "/").Replace("\\n", " ").Replace("\\t", " ").Replace(
                //     "\\r", "").Replace("e\"", "e\" ").Replace("\"style", "\" style");
                var id = jobj["inventoryId"].Value<string>();
                if (id == "BodyArmour")
                {
                    AddItem(jobj, ItemClass.Armor, ItemSlot.Armor);
                }
                if (id == "Ring")
                {
                    AddItem(jobj, ItemClass.Ring, ItemSlot.Ring);
                }
                if (id == "Ring2")
                {
                    AddItem(jobj, ItemClass.Ring, ItemSlot.Ring2);
                }
                if (id == "Gloves")
                {
                    AddItem(jobj, ItemClass.Gloves, ItemSlot.Gloves);
                }
                if (id == "Weapon")
                {
                    AddItem(jobj, ItemClass.MainHand, ItemSlot.MainHand);
                }
                if (id == "Offhand")
                {
                    AddItem(jobj, ItemClass.OffHand, ItemSlot.OffHand);
                }
                if (id == "Helm")
                {
                    AddItem(jobj, ItemClass.Helm, ItemSlot.Helm);
                }
                if (id == "Boots")
                {
                    AddItem(jobj, ItemClass.Boots, ItemSlot.Boots);
                }
                if (id == "Amulet")
                {
                    AddItem(jobj, ItemClass.Amulet, ItemSlot.Amulet);
                }
                if (id == "Belt")
                {
                    AddItem(jobj, ItemClass.Belt, ItemSlot.Belt);
                }
            }

            #endregion

            RefreshItemAttributes();
        }

        private void RefreshItemAttributes()
        {
            aList.Clear();
            NonLocalMods.Clear();
            Attributes = new ListCollectionView(aList);
            foreach (Item item in Equip)
            {
                LoadItem(item, aList, NonLocalMods);
            }

            var pgd = new PropertyGroupDescription("Group");
            pgd.Converter = new HeaderConverter();
            Attributes.GroupDescriptions.Add(pgd);
            Attributes.CustomSort = new NumberLessStringComparer();

            Attributes.Refresh();
        }

        public static void LoadItem(Item item, List<Attribute> attributes,List<Attribute> nonlocal)
        {
            foreach (var attr in item.Attributes)
            {
                if (attr.Key == "Quality: #") continue;
                attributes.Add(new Attribute(attr.Key, attr.Value, item.Class.ToString()));
            }

            foreach (ItemMod mod in item.Mods)
            {
                Attribute attTo = null;
                attTo =
                    attributes.Find(
                        ad =>
                            ad.TextAttribute == mod.Attribute &&
                            ad.Group == (mod.isLocal ? item.Class.ToString() : "Independent"));
                if (attTo == null)
                {
                    attributes.Add(new Attribute(mod.Attribute, mod.Value,
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
                attTo = nonlocal.Find(ad => ad.TextAttribute == attr.Key);
                if (attTo == null)
                {
                    nonlocal.Add(new Attribute(attr.Key, attr.Value, ""));
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
                attTo = nonlocal.Find(ad => ad.TextAttribute == mod.Attribute);
                if (attTo == null)
                {
                    nonlocal.Add(new Attribute(mod.Attribute, mod.Value, ""));
                }
                else
                {
                    attTo.Add(mod.Attribute, mod.Value);
                }
            }
        }

        private void AddItem(JObject val, ItemClass iclass, ItemSlot islot)
        {
            Item item = null;
            item = new Item(iclass, val);
            item.Slot = islot;
            Equip.Add(item);
            OnPropertyChanged("Equip");

            RefreshItemAttributes();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public class Attribute : INotifyPropertyChanged
        {
            private readonly string _attribute;
            private static readonly Regex _backreplace = new Regex("#");

            public static Regex Backreplace
            {
                get { return _backreplace; }
            }

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
                    if (((Attribute)x).Group == "Independent" && !(((Attribute)y).Group == "Independent")) return +1;
                    if (((Attribute)y).Group == "Independent" && !(((Attribute)x).Group == "Independent")) return -1;
                    return
                        numberfilter.Replace(((Attribute)y).Group, "")
                            .CompareTo(numberfilter.Replace(((Attribute)y).Group, ""));
                }
                return 0;
            }

            public int Compare(string x, string y)
            {
                return numberfilter.Replace(x, "").CompareTo(numberfilter.Replace(y, ""));
            }
        }


        public class HeaderConverter : IValueConverter
        {
            public Dictionary<string, AttributeGroup> ItemGroups = new Dictionary<string, AttributeGroup>();

            public HeaderConverter()
            {
                foreach (var itemClass in Enum.GetValues(typeof(ItemClass)))
                {
                    if (!ItemGroups.ContainsKey(itemClass.ToString()))
                    {
                        ItemGroups.Add(itemClass.ToString(), new AttributeGroup(itemClass.ToString()));
                    }
                }

                ItemGroups.Add("Independent", new AttributeGroup("Independent"));
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ItemGroups[value.ToString()];
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}