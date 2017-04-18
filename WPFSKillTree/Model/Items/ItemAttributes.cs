using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
    public class ItemAttributes : Notifier
    {
        #region slotted items

        public Item Armor
        {
            get { return GetItemInSlot(ItemSlot.BodyArmour); }
            set { SetItemInSlot(value, ItemSlot.BodyArmour); }
        }

        public Item MainHand
        {
            get { return GetItemInSlot(ItemSlot.MainHand); }
            set { SetItemInSlot(value, ItemSlot.MainHand); }
        }

        public Item OffHand
        {
            get { return GetItemInSlot(ItemSlot.OffHand); }
            set { SetItemInSlot(value, ItemSlot.OffHand); }
        }

        public Item Ring
        {
            get { return GetItemInSlot(ItemSlot.Ring); }
            set { SetItemInSlot(value, ItemSlot.Ring); }
        }

        public Item Ring2
        {
            get { return GetItemInSlot(ItemSlot.Ring2); }
            set { SetItemInSlot(value, ItemSlot.Ring2); }
        }

        public Item Amulet
        {
            get { return GetItemInSlot(ItemSlot.Amulet); }
            set { SetItemInSlot(value, ItemSlot.Amulet); }
        }

        public Item Helm
        {
            get { return GetItemInSlot(ItemSlot.Helm); }
            set { SetItemInSlot(value, ItemSlot.Helm); }
        }

        public Item Gloves
        {
            get { return GetItemInSlot(ItemSlot.Gloves); }
            set { SetItemInSlot(value, ItemSlot.Gloves); }
        }

        public Item Boots
        {
            get { return GetItemInSlot(ItemSlot.Boots); }
            set { SetItemInSlot(value, ItemSlot.Boots); }
        }

        public Item Belt
        {
            get { return GetItemInSlot(ItemSlot.Belt); }
            set { SetItemInSlot(value, ItemSlot.Belt); }
        }

        public Item Jewel
        {
            get { return GetItemInSlot(ItemSlot.Jewel); }
            set { SetItemInSlot(value, ItemSlot.Jewel); }
        }

        public Item GetItemInSlot(ItemSlot slot)
        {
            return Equip.FirstOrDefault(i => i.Slot == slot);
        }

        public void SetItemInSlot(Item value, ItemSlot slot)
        {
            if (!CanEquip(value, slot))
                return;
            
            var old = Equip.FirstOrDefault(i => i.Slot == slot);
            if (old != null)
            {
                Equip.Remove(old);
                old.Slot = ItemSlot.Unequipable;
                old.PropertyChanged -= SlottedItemOnPropertyChanged;
            }

            if (value != null)
            {
                value.Slot = slot;
                Equip.Add(value);
                value.PropertyChanged += SlottedItemOnPropertyChanged;
            }
            OnPropertyChanged(slot.ToString());
            RefreshItemAttributes();
        }

        public bool CanEquip(Item item, ItemSlot slot)
        {
            if (item == null) return true;
            if (slot == ItemSlot.Unequipable) return false;
            // one handed -> only equippable if other hand is free, shield or matching one handed
            if (item.ItemGroup == ItemGroup.OneHandedWeapon
                && (slot == ItemSlot.MainHand || slot == ItemSlot.OffHand))
            {
                var other = slot == ItemSlot.MainHand ? OffHand : MainHand;
                if (other == null || other.ItemGroup == ItemGroup.Shield)
                    return true;
                if (other.ItemGroup != ItemGroup.OneHandedWeapon)
                    return false;
                if ((item.ItemType == ItemType.Wand && other.ItemType != ItemType.Wand)
                    || (other.ItemType == ItemType.Wand && item.ItemType != ItemType.Wand))
                    return false;
                return true;
            }
            // two handed and not bow -> only equippable if off hand is free
            if (item.ItemGroup == ItemGroup.TwoHandedWeapon && item.ItemType != ItemType.Bow
                && slot == ItemSlot.MainHand)
            {
                return OffHand == null;
            }
            // bow -> only equippable if off hand is free or quiver
            if (item.ItemType == ItemType.Bow && slot == ItemSlot.MainHand)
            {
                return OffHand == null || OffHand.ItemGroup == ItemGroup.Quiver;
            }
            // quiver -> only equippable if main hand is free or bow
            if (item.ItemGroup == ItemGroup.Quiver && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.ItemType == ItemType.Bow;
            }
            // shield -> only equippable if main hand is free or one hand
            if (item.ItemGroup == ItemGroup.Shield && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.ItemGroup == ItemGroup.OneHandedWeapon;
            }
            return ((int)item.ItemGroup.ItemSlots() & (int)slot) != 0;
        }
        #endregion

        public ObservableCollection<Item> Equip { get; }

        private ListCollectionView _attributes;
        public ListCollectionView Attributes
        {
            get { return _attributes; }
            private set { SetProperty(ref _attributes, value); }
        }

        public IReadOnlyList<ItemMod> NonLocalMods { get; private set; }

        private readonly IPersistentData _persistentData;

        public event EventHandler ItemDataChanged;

        private void SlottedItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Item.JsonBase))
            {
                ItemDataChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ItemAttributes()
        {
            Equip = new ObservableCollection<Item>();
            RefreshItemAttributes();
        }

        public ItemAttributes(IPersistentData persistentData, string itemData)
        {
            _persistentData = persistentData;
            Equip = new ObservableCollection<Item>();

            var jObject = JObject.Parse(itemData);
            foreach (JObject jobj in (JArray)jObject["items"])
            {
                switch (jobj["inventoryId"].Value<string>())
                {
                    case "BodyArmour":
                        AddItem(jobj, ItemSlot.BodyArmour);
                        break;
                    case "Ring":
                        AddItem(jobj, ItemSlot.Ring);
                        break;
                    case "Ring2":
                        AddItem(jobj, ItemSlot.Ring2);
                        break;
                    case "Gloves":
                        AddItem(jobj, ItemSlot.Gloves);
                        break;
                    case "Weapon":
                        AddItem(jobj, ItemSlot.MainHand);
                        break;
                    case "Offhand":
                        AddItem(jobj, ItemSlot.OffHand);
                        break;
                    case "Helm":
                        AddItem(jobj, ItemSlot.Helm);
                        break;
                    case "Boots":
                        AddItem(jobj, ItemSlot.Boots);
                        break;
                    case "Amulet":
                        AddItem(jobj, ItemSlot.Amulet);
                        break;
                    case "Belt":
                        AddItem(jobj, ItemSlot.Belt);
                        break;
                    case "Jewel":
                        AddItem(jobj, ItemSlot.Jewel);
                        break;
                }
            }

            RefreshItemAttributes();
        }

        public string ToJsonString()
        {
            var items = new JArray();
            foreach (var item in Equip)
            {
                var jItem = item.JsonBase;
                switch (item.Slot)
                {
                    case ItemSlot.BodyArmour:
                        jItem["inventoryId"] = "BodyArmour";
                        break;
                    case ItemSlot.MainHand:
                        jItem["inventoryId"] = "Weapon";
                        break;
                    case ItemSlot.OffHand:
                        jItem["inventoryId"] = "Offhand";
                        break;
                    case ItemSlot.Ring:
                        jItem["inventoryId"] = "Ring";
                        break;
                    case ItemSlot.Ring2:
                        jItem["inventoryId"] = "Ring2";
                        break;
                    case ItemSlot.Amulet:
                        jItem["inventoryId"] = "Amulet";
                        break;
                    case ItemSlot.Helm:
                        jItem["inventoryId"] = "Helm";
                        break;
                    case ItemSlot.Gloves:
                        jItem["inventoryId"] = "Gloves";
                        break;
                    case ItemSlot.Boots:
                        jItem["inventoryId"] = "Boots";
                        break;
                    case ItemSlot.Belt:
                        jItem["inventoryId"] = "Belt";
                        break;
                    case ItemSlot.Jewel:
                        jItem["inventoryId"] = "Jewel";
                        break;
                }
                items.Add(jItem);
            }
            var jObj = new JObject();
            jObj["items"] = items;
            return jObj.ToString(Formatting.None);
        }

        private void RefreshItemAttributes()
        {
            NonLocalMods = (from item in Equip
                            from mod in SelectNonLocalMods(item)
                            group mod by mod.Attribute into modsForAttr
                            select modsForAttr.Aggregate((m1, m2) => m1.Sum(m2))
                           ).ToList();
            var aList = new List<Attribute>();
            var independent = new List<Attribute>();
            foreach (var item in Equip)
            {
                LoadItemAttributes(item, aList, independent);
            }
            aList.AddRange(independent);
            Attributes = new ListCollectionView(aList);

            var pgd = new PropertyGroupDescription("Group", new HeaderConverter());
            Attributes.GroupDescriptions.Add(pgd);

            Attributes.Refresh();
        }

        private static void AddAttribute(ItemMod mod, string group, ICollection<Attribute> attributes, Attribute existingAttribute)
        {
            if (existingAttribute == null)
            {
                attributes.Add(new Attribute(mod.Attribute, mod.Value, group));
            }
            else
            {
                existingAttribute.Add(mod.Value);
            }
        }

        private static void LoadItemAttributes(Item item, List<Attribute> attributes, List<Attribute> independentAttributes)
        {
            foreach (var attr in item.Properties)
            {
                // Show all properties except quality in the group for this slot.
                if (attr.Attribute == "Quality: +#%") continue;
                attributes.Add(new Attribute(attr.Attribute, attr.Value, item.Slot.ToString()));
            }

            var modsAffectingProperties = item.GetModsAffectingProperties().SelectMany(pair => pair.Value).ToList();
            foreach (var mod in item.Mods)
            {
                if (mod.IsLocal)
                {
                    // Show local mods in the group for this slot
                    // if they are not already represented by affecting properties.
                    if (mod.Attribute.StartsWith("Adds") || modsAffectingProperties.Contains(mod))
                        continue;
                    var attTo = attributes.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == item.Slot.ToString());
                    AddAttribute(mod, item.Slot.ToString(), attributes, attTo);
                }
                else
                {
                    // Show all non-local mods in the Independent group.
                    var attTo = independentAttributes.Find(ad => ad.TextAttribute == mod.Attribute && ad.Group == "Independent");
                    AddAttribute(mod, "Independent", independentAttributes, attTo);
                }
            }
        }

        private static IEnumerable<ItemMod> SelectNonLocalMods(Item item)
        {
            var mods = item.Mods.Where(m => !m.IsLocal);
            // Weapons are treated differently, their properties do not count towards global mods.
            if (item.ItemGroup != ItemGroup.OneHandedWeapon && item.ItemGroup != ItemGroup.TwoHandedWeapon)
                return mods.Union(item.Properties.Where(p => p.Attribute != "Quality: +#%"));
            return mods;
        }

        private void AddItem(JObject val, ItemSlot islot)
        {
            var item = new Item(_persistentData, val, islot);
            Equip.Add(item);
            item.PropertyChanged += SlottedItemOnPropertyChanged;
        }


        public class Attribute : Notifier
        {
            public static readonly Regex Backreplace = new Regex("#");

            private readonly List<float> _value;

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


        private class HeaderConverter : IValueConverter
        {
            private readonly Dictionary<string, AttributeGroup> _itemGroups = new Dictionary<string, AttributeGroup>();

            public HeaderConverter()
            {
                foreach (var slot in Enum.GetValues(typeof(ItemSlot)))
                {
                    if (!_itemGroups.ContainsKey(slot.ToString()))
                    {
                        _itemGroups.Add(slot.ToString(), new AttributeGroup(slot.ToString()));
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