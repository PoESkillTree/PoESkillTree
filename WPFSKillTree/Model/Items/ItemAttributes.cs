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
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    using SmallDigit =
#if (PoESkillTree_StoreItemAttributesInSmallDec)
    SmallDec;
#else
    System.Single;
#endif
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

        public Item Jewel02
        {
            get { return GetItemInSlot(ItemSlot.Jewel02); }
            set { SetItemInSlot(value, ItemSlot.Jewel02); }
        }
		
        public Item Jewel03
        {
            get { return GetItemInSlot(ItemSlot.Jewel03); }
            set { SetItemInSlot(value, ItemSlot.Jewel03); }
        }

        public Item Jewel04
        {
            get { return GetItemInSlot(ItemSlot.Jewel04); }
            set { SetItemInSlot(value, ItemSlot.Jewel04); }
        }

        public Item Jewel05
        {
            get { return GetItemInSlot(ItemSlot.Jewel05); }
            set { SetItemInSlot(value, ItemSlot.Jewel05); }
        }
		
        public Item Jewel06
        {
            get { return GetItemInSlot(ItemSlot.Jewel06); }
            set { SetItemInSlot(value, ItemSlot.Jewel06); }
        }
		
	    public Item Jewel07
        {
            get { return GetItemInSlot(ItemSlot.Jewel07); }
            set { SetItemInSlot(value, ItemSlot.Jewel07); }
        }

        public Item Jewel08
        {
            get { return GetItemInSlot(ItemSlot.Jewel08); }
            set { SetItemInSlot(value, ItemSlot.Jewel08); }
        }
		
        public Item Jewel09
        {
            get { return GetItemInSlot(ItemSlot.Jewel09); }
            set { SetItemInSlot(value, ItemSlot.Jewel09); }
        }
		
        public Item Jewel10
        {
            get { return GetItemInSlot(ItemSlot.Jewel10); }
            set { SetItemInSlot(value, ItemSlot.Jewel10); }
        }
		
		public Item Jewel11
        {
            get { return GetItemInSlot(ItemSlot.Jewel11); }
            set { SetItemInSlot(value, ItemSlot.Jewel11); }
        }
		
        public Item Jewel12
        {
            get { return GetItemInSlot(ItemSlot.Jewel12); }
            set { SetItemInSlot(value, ItemSlot.Jewel12); }
        }
		
        public Item Jewel13
        {
            get { return GetItemInSlot(ItemSlot.Jewel13); }
            set { SetItemInSlot(value, ItemSlot.Jewel13); }
        }

        public Item Jewel14
        {
            get { return GetItemInSlot(ItemSlot.Jewel14); }
            set { SetItemInSlot(value, ItemSlot.Jewel14); }
        }

        public Item Jewel15
        {
            get { return GetItemInSlot(ItemSlot.Jewel15); }
            set { SetItemInSlot(value, ItemSlot.Jewel15); }
        }
		
        public Item Jewel16
        {
            get { return GetItemInSlot(ItemSlot.Jewel16); }
            set { SetItemInSlot(value, ItemSlot.Jewel16); }
        }
		
	    public Item Jewel17
        {
            get { return GetItemInSlot(ItemSlot.Jewel17); }
            set { SetItemInSlot(value, ItemSlot.Jewel17); }
        }

        public Item Jewel18
        {
            get { return GetItemInSlot(ItemSlot.Jewel18); }
            set { SetItemInSlot(value, ItemSlot.Jewel18); }
        }
		
        public Item Jewel19
        {
            get { return GetItemInSlot(ItemSlot.Jewel19); }
            set { SetItemInSlot(value, ItemSlot.Jewel19); }
        }
		
		public Item Jewel20
        {
            get { return GetItemInSlot(ItemSlot.Jewel20); }
            set { SetItemInSlot(value, ItemSlot.Jewel20); }
        }
		
		public Item Jewel21
        {
            get { return GetItemInSlot(ItemSlot.Jewel21); }
            set { SetItemInSlot(value, ItemSlot.Jewel21); }
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
            if (item.Tags.HasFlag(Tags.OneHand)
                && (slot == ItemSlot.MainHand || slot == ItemSlot.OffHand))
            {
                var other = slot == ItemSlot.MainHand ? OffHand : MainHand;
                if (other == null || other.ItemClass == ItemClass.Shield)
                    return true;
                if (!other.Tags.HasFlag(Tags.OneHand))
                    return false;
                if ((item.ItemClass == ItemClass.Wand && other.ItemClass != ItemClass.Wand)
                    || (other.ItemClass == ItemClass.Wand && item.ItemClass != ItemClass.Wand))
                    return false;
                return true;
            }
            // two handed and not bow -> only equippable if off hand is free
            if (item.Tags.HasFlag(Tags.TwoHand) && item.ItemClass != ItemClass.Bow
                && slot == ItemSlot.MainHand)
            {
                return OffHand == null;
            }
            // bow -> only equippable if off hand is free or quiver
            if (item.ItemClass == ItemClass.Bow && slot == ItemSlot.MainHand)
            {
                return OffHand == null || OffHand.ItemClass == ItemClass.Quiver;
            }
            // quiver -> only equippable if main hand is free or bow
            if (item.ItemClass == ItemClass.Quiver && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.ItemClass == ItemClass.Bow;
            }
            // shield -> only equippable if main hand is free or one hand
            if (item.ItemClass == ItemClass.Shield && slot == ItemSlot.OffHand)
            {
                return MainHand == null || MainHand.Tags.HasFlag(Tags.OneHand);
            }
            return ((int) item.ItemClass.ItemSlots() & (int) slot) != 0;
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
                    case "Jewel02":
                        AddItem(jobj, ItemSlot.Jewel02);
                        break;
                    case "Jewel03":
                        AddItem(jobj, ItemSlot.Jewel03);
                        break;
                    case "Jewel04":
                        AddItem(jobj, ItemSlot.Jewel04);
                        break;
                    case "Jewel05":
                        AddItem(jobj, ItemSlot.Jewel05);
                        break;
                    case "Jewel06":
                        AddItem(jobj, ItemSlot.Jewel06);
                        break;
                    case "Jewel07":
                        AddItem(jobj, ItemSlot.Jewel07);
                        break;
                    case "Jewel08":
                        AddItem(jobj, ItemSlot.Jewel08);
                        break;
                    case "Jewel09":
                        AddItem(jobj, ItemSlot.Jewel09);
                        break;
                    case "Jewel10":
                        AddItem(jobj, ItemSlot.Jewel10);
                        break;
                    case "Jewel11":
                        AddItem(jobj, ItemSlot.Jewel11);
                        break;
                    case "Jewel12":
                        AddItem(jobj, ItemSlot.Jewel12);
                        break;
                    case "Jewel13":
                        AddItem(jobj, ItemSlot.Jewel13);
                        break;
                    case "Jewel14":
                        AddItem(jobj, ItemSlot.Jewel14);
                        break;
                    case "Jewel15":
                        AddItem(jobj, ItemSlot.Jewel15);
                        break;
                    case "Jewel16":
                        AddItem(jobj, ItemSlot.Jewel16);
                        break;
                    case "Jewel17":
                        AddItem(jobj, ItemSlot.Jewel17);
                        break;
                    case "Jewel18":
                        AddItem(jobj, ItemSlot.Jewel18);
                        break;
                    case "Jewel19":
                        AddItem(jobj, ItemSlot.Jewel19);
                        break;
                    case "Jewel20":
                        AddItem(jobj, ItemSlot.Jewel20);
                        break;
                    case "Jewel21":
                        AddItem(jobj, ItemSlot.Jewel21);
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
                    case ItemSlot.Jewel02:
                        jItem["inventoryId"] = "Jewel02";
                        break;
                    case ItemSlot.Jewel03:
                        jItem["inventoryId"] = "Jewel03";
                        break;
                    case ItemSlot.Jewel04:
                        jItem["inventoryId"] = "Jewel04";
                        break;
                    case ItemSlot.Jewel05:
                        jItem["inventoryId"] = "Jewel05";
                        break;
                    case ItemSlot.Jewel06:
                        jItem["inventoryId"] = "Jewel06";
                        break;
                    case ItemSlot.Jewel07:
                        jItem["inventoryId"] = "Jewel07";
                        break;
                    case ItemSlot.Jewel08:
                        jItem["inventoryId"] = "Jewel08";
                        break;
                    case ItemSlot.Jewel09:
                        jItem["inventoryId"] = "Jewel09";
                        break;
                    case ItemSlot.Jewel10:
                        jItem["inventoryId"] = "Jewel10";
                        break;
                    case ItemSlot.Jewel11:
                        jItem["inventoryId"] = "Jewel11";
                        break;
                    case ItemSlot.Jewel12:
                        jItem["inventoryId"] = "Jewel12";
                        break;
                    case ItemSlot.Jewel13:
                        jItem["inventoryId"] = "Jewel13";
                        break;
                    case ItemSlot.Jewel14:
                        jItem["inventoryId"] = "Jewel14";
                        break;
                    case ItemSlot.Jewel15:
                        jItem["inventoryId"] = "Jewel15";
                        break;
                    case ItemSlot.Jewel16:
                        jItem["inventoryId"] = "Jewel16";
                        break;
                    case ItemSlot.Jewel17:
                        jItem["inventoryId"] = "Jewel17";
                        break;
                    case ItemSlot.Jewel18:
                        jItem["inventoryId"] = "Jewel18";
                        break;
                    case ItemSlot.Jewel19:
                        jItem["inventoryId"] = "Jewel19";
                        break;
                    case ItemSlot.Jewel20:
                        jItem["inventoryId"] = "Jewel20";
                        break;
                    case ItemSlot.Jewel21:
                        jItem["inventoryId"] = "Jewel21";
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
                attributes.Add(new Attribute(mod.Attribute, mod.Values, group));
            }
            else
            {
                existingAttribute.Add(mod.Values);
            }
        }

        private static void LoadItemAttributes(Item item, List<Attribute> attributes, List<Attribute> independentAttributes)
        {
            foreach (var attr in item.Properties)
            {
                // Show all properties except quality in the group for this slot.
                if (attr.Attribute == "Quality: +#%") continue;
                attributes.Add(new Attribute(attr.Attribute, attr.Values, item.Slot.ToString()));
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
            if (!item.Tags.HasFlag(Tags.Weapon))
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

            private readonly List<SmallDigit> _value;

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

            public Attribute(string s, IEnumerable<SmallDigit> val, string grp)
            {
                _attribute = s;
                _value = new List<SmallDigit>(val);
                _group = grp;
            }

            public void Add(IReadOnlyList<SmallDigit> val)
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