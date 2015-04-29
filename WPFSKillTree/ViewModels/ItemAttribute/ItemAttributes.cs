using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Raven.Json.Linq;
using System.Linq;
using System;

namespace POESKillTree.ViewModels.ItemAttribute
{
    public class ItemAttributes : INotifyPropertyChanged
    {
        #region slotted items
        public Item Armor
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Armor);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Armor);
            }
        }
        public Item MainHand
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.MainHand);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.MainHand);
            }
        }
        public Item OffHand
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.OffHand);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.OffHand);
            }
        }
        public Item Ring
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Ring);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Ring);
            }
        }
        public Item Ring2
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Ring2);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Ring2);
            }
        }
        public Item Helm
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Helm);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Helm);
            }
        }
        public Item Gloves
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Gloves);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Gloves);
            }
        }
        public Item Boots
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Boots);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Boots);
            }
        }
        public Item Gem
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Gem);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Gem);
            }
        }
        public Item Belt
        {
            get
            {
                return GetItemInSlot(Item.ItemSlot.Belt);
            }
            set
            {
                SetItemInSlot(value, Item.ItemSlot.Belt);
            }
        }


        public Item GetItemInSlot(Item.ItemSlot slot)
        {
            return _Equip.FirstOrDefault(i => i.Slot == slot);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="slot"></param>
        /// <returns>true if item was set</returns>
        public bool SetItemInSlot(Item value, Item.ItemSlot slot)
        {
            if (value == null)
                return false;

            if (value.Class == Item.ItemClass.Unequipable)
                return false;

            if (slot == Item.ItemSlot.Unequiped)
                return false;

            if ((int)value.Class != (int)slot && (slot == Item.ItemSlot.Ring2 && value.Class != Item.ItemClass.Ring))
                return false;

            RemoveItemFromSlot(slot);

            value.Slot = slot;
            _Equip.Add(value);
            OnPropertyChanged(slot.ToString());
            return true;
        }


        public Item RemoveItemFromSlot(Item.ItemSlot slot)
        {
            var itm = _Equip.FirstOrDefault(i => i.Slot == slot);
            if (itm != null)
                _Equip.Remove(itm);

            OnPropertyChanged("Equip");
            OnPropertyChanged(slot.ToString());

            itm.Slot = Item.ItemSlot.Unequiped;
            return itm;
        }
        #endregion

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
            foreach (RavenJObject jobj in (RavenJArray)jObject["items"])
            {
                var html = jobj["x"].Value<string>();
                //html =
                //   html.Replace("\\\"", "\"").Replace("\\/", "/").Replace("\\n", " ").Replace("\\t", " ").Replace(
                //     "\\r", "").Replace("e\"", "e\" ").Replace("\"style", "\" style");
                var id = jobj["inventoryId"].Value<string>();
                if (id == "BodyArmour")
                {
                    AddItem(jobj, Item.ItemClass.Armor, Item.ItemSlot.Armor);
                }
                if (id == "Ring")
                {
                    AddItem(jobj, Item.ItemClass.Ring, Item.ItemSlot.Ring);
                }
                if (id == "Ring2")
                {
                    AddItem(jobj, Item.ItemClass.Ring, Item.ItemSlot.Ring2);
                }
                if (id == "Gloves")
                {
                    AddItem(jobj, Item.ItemClass.Gloves, Item.ItemSlot.Gloves);
                }
                if (id == "Weapon")
                {
                    AddItem(jobj, Item.ItemClass.MainHand, Item.ItemSlot.MainHand);
                }
                if (id == "Offhand")
                {
                    AddItem(jobj, Item.ItemClass.OffHand, Item.ItemSlot.OffHand);
                }
                if (id == "Helm")
                {
                    AddItem(jobj, Item.ItemClass.Helm, Item.ItemSlot.Helm);
                }
                if (id == "Boots")
                {
                    AddItem(jobj, Item.ItemClass.Boots, Item.ItemSlot.Boots);
                }
                if (id == "Amulet")
                {
                    AddItem(jobj, Item.ItemClass.Amulet, Item.ItemSlot.Amulet);
                }
                if (id == "Belt")
                {
                    AddItem(jobj, Item.ItemClass.Belt, Item.ItemSlot.Belt);
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

        private void AddItem(RavenJObject val, Item.ItemClass iclass, Item.ItemSlot islot)
        {
            Item item = null;
            item = new Item(iclass, val);
            item.Slot = islot;
            Equip.Add(item);
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}