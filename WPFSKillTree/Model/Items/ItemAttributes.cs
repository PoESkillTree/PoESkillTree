using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
    public class ItemAttributes : Notifier
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

        private Item GetItemInSlot(ItemSlot slot)
        {
            return Equip.FirstOrDefault(i => i.Slot == slot);
        }

        private void SetItemInSlot(Item value, ItemSlot slot)
        {
            if (value != null && ((int)value.Class & (int) slot) == 0)
                return;
            if (slot == ItemSlot.Unequipable)
                return;
            RemoveItemFromSlot(slot);

            if (value != null)
            {
                value.Slot = slot;
                Equip.Add(value);
            }
            OnPropertyChanged(slot.ToString());
            RefreshItemAttributes();
        }

        private void RemoveItemFromSlot(ItemSlot slot)
        {
            var itm = Equip.FirstOrDefault(i => i.Slot == slot);
            if (itm != null)
                Equip.Remove(itm);

            OnPropertyChanged(slot.ToString());

            if (itm != null)
                itm.Slot = ItemSlot.Unequipable;
        }
        #endregion

        private readonly List<Attribute> _aList = new List<Attribute>();

        public ObservableCollection<Item> Equip { get; private set; }

        private ListCollectionView _attributes;
        public ListCollectionView Attributes
        {
            get { return _attributes; }
            private set { SetProperty(ref _attributes, value); }
        }

        private readonly List<Attribute> _nonLocalMods = new List<Attribute>();
        public IReadOnlyList<Attribute> NonLocalMods
        {
            get { return _nonLocalMods; }
        }

        public ItemAttributes()
        {
            Equip = new ObservableCollection<Item>();
            RefreshItemAttributes();
        }

        public ItemAttributes(string itemData)
        {
            Equip = new ObservableCollection<Item>();

            var jObject = JObject.Parse(itemData);
            foreach (JObject jobj in (JArray)jObject["items"])
            {
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

            RefreshItemAttributes();
        }

        private void RefreshItemAttributes()
        {
            _aList.Clear();
            _nonLocalMods.Clear();
            Attributes = new ListCollectionView(_aList);
            foreach (var item in Equip)
            {
                LoadItem(item, _aList, _nonLocalMods);
            }

            var pgd = new PropertyGroupDescription("Group", new HeaderConverter());
            Attributes.GroupDescriptions.Add(pgd);
            Attributes.CustomSort = new NumberLessStringComparer();

            Attributes.Refresh();
        }

        public static void LoadItem(Item item, List<Attribute> attributes, List<Attribute> nonlocal)
        {
            foreach (var attr in item.Attributes)
            {
                if (attr.Key == "Quality: #") continue;
                attributes.Add(new Attribute(attr.Key, attr.Value, item.Class.ToString()));
            }

            foreach (ItemMod mod in item.Mods)
            {
                var attTo = attributes.Find(
                    ad =>
                        ad.TextAttribute == mod.Attribute &&
                        ad.Group == (mod.IsLocal ? item.Class.ToString() : "Independent"));
                if (attTo == null)
                {
                    attributes.Add(new Attribute(mod.Attribute, mod.Value,
                        mod.IsLocal ? item.Class.ToString() : "Independent"));
                }
                else
                {
                    attTo.Add(mod.Value);
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
                var attTo = nonlocal.Find(ad => ad.TextAttribute == attr.Key);
                if (attTo == null)
                {
                    nonlocal.Add(new Attribute(attr.Key, attr.Value, ""));
                }
                else
                {
                    attTo.Add(attr.Value);
                }
            }

            foreach (ItemMod mod in item.Mods)
            {
                if (mod.IsLocal) continue;
                var attTo = nonlocal.Find(ad => ad.TextAttribute == mod.Attribute);
                if (attTo == null)
                {
                    nonlocal.Add(new Attribute(mod.Attribute, mod.Value, ""));
                }
                else
                {
                    attTo.Add(mod.Value);
                }
            }
        }

        private void AddItem(JObject val, ItemClass iclass, ItemSlot islot)
        {
            var item = new Item(iclass, val) {Slot = islot};
            Equip.Add(item);

            RefreshItemAttributes();
        }


        public class Attribute : Notifier
        {
            public static readonly Regex Backreplace = new Regex("#");

            private readonly List<float> _value;
            public IReadOnlyList<float> Value
            {
                get { return _value; }
            }

            private readonly string _group;
            public string Group
            {
                get { return _group; }
            }

            private readonly string _attribute;
            public string TextAttribute
            {
                get { return _attribute; }
            }

            public string ValuedAttribute
            {
                get { return _value.Aggregate(_attribute, (current, f) => Backreplace.Replace(current, f + "", 1)); }
            }

            public Attribute(string s, IEnumerable<float> val, string grp)
            {
                _attribute = s;
                _value = new List<float>(val);
                _group = grp;
            }

            public void Add(IReadOnlyList<float> val)
            {
                if (_value.Count != val.Count) throw new NotSupportedException();
                for (var i = 0; i < val.Count; i++)
                {
                    _value[i] += val[i];
                }
                OnPropertyChanged("ValuedAttribute");
            }
        }


        private class NumberLessStringComparer : IComparer, IComparer<Attribute>
        {
            private static readonly Regex Numberfilter = new Regex("[0-9]*\\.?[0-9]+");

            public int Compare(object x, object y)
            {
                return Compare(x as Attribute, y as Attribute);
            }

            public int Compare(Attribute xAttr, Attribute yAttr)
            {
                if (xAttr == null || yAttr == null) return 0;
                var xGroup = xAttr.Group;
                var yGroup = yAttr.Group;
                if (xGroup == "Independent" && yGroup != "Independent") return +1;
                if (yGroup == "Independent" && xGroup != "Independent") return -1;
                return Numberfilter.Replace(xGroup, "").CompareTo(Numberfilter.Replace(yGroup, ""));
            }
        }


        private class HeaderConverter : IValueConverter
        {
            private readonly Dictionary<string, AttributeGroup> _itemGroups = new Dictionary<string, AttributeGroup>();

            public HeaderConverter()
            {
                foreach (var itemClass in Enum.GetValues(typeof(ItemClass)))
                {
                    if (!_itemGroups.ContainsKey(itemClass.ToString()))
                    {
                        _itemGroups.Add(itemClass.ToString(), new AttributeGroup(itemClass.ToString()));
                    }
                }

                _itemGroups.Add("Independent", new AttributeGroup("Independent"));
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return _itemGroups[value.ToString()];
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }
    }
}