using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace POESKillTree.Model.Items
{
    public class ItemAttributes : Notifier, IDisposable
    {
        #region slotted items

        private Item MainHand => GetItemInSlot(ItemSlot.MainHand);

        private Item OffHand => GetItemInSlot(ItemSlot.OffHand);

        public Item GetItemInSlot(ItemSlot slot)
            => Equip.FirstOrDefault(i => i.Slot == slot);

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

        public IReadOnlyList<Skill> GetSkillsInSlot(ItemSlot slot)
            => Skills.Where(ss => ss.Any(s => s.ItemSlot == slot)).DefaultIfEmpty(new Skill[0]).First();

        public void SetSkillsInSlot(IReadOnlyList<Skill> value, ItemSlot slot)
        {
            var oldValue = Skills.FirstOrDefault(ss => ss.Any(s => s.ItemSlot == slot));
            if (oldValue != null)
            {
                Skills.Remove(oldValue);
            }
            Skills.Add(value);
        }

        private void AddSkillsToSlot(IEnumerable<Skill> skills, ItemSlot slot)
            => SetSkillsInSlot(GetSkillsInSlot(slot).Concat(skills).ToList(), slot);

        #endregion

        public ObservableCollection<Item> Equip { get; } = new ObservableCollection<Item>();

        public ObservableCollection<IReadOnlyList<Skill>> Skills { get; } =
            new ObservableCollection<IReadOnlyList<Skill>>();

        private ListCollectionView _attributes;
        public ListCollectionView Attributes
        {
            get => _attributes;
            private set => SetProperty(ref _attributes, value);
        }

        public IReadOnlyList<ItemMod> NonLocalMods { get; private set; }

        private readonly EquipmentData _equipmentData;
        private readonly SkillDefinitions _skillDefinitions;

        public event EventHandler ItemDataChanged;

        public ItemAttributes(EquipmentData equipmentData, SkillDefinitions skillDefinitions, string itemData = null)
        {
            _equipmentData = equipmentData;
            _skillDefinitions = skillDefinitions;
            Equip.CollectionChanged += OnCollectionChanged;
            SetSkillsInSlot(new[] { Skill.Default, }, ItemSlot.Unequipable);
            Skills.CollectionChanged += OnCollectionChanged;

            if (!string.IsNullOrEmpty(itemData))
            {
                var jObject = JObject.Parse(itemData);
                DeserializeItems(jObject);
                DeserializeSkills(jObject);
            }

            RefreshItemAttributes();
        }

        private void DeserializeItems(JObject itemData)
        {
            if (!itemData.TryGetValue("items", out var itemJson))
                return;

            foreach (JObject jobj in (JArray) itemJson)
            {
                var inventoryId = jobj.Value<string>("inventoryId");
                switch (inventoryId)
                {
                    case "Weapon":
                        inventoryId = "MainHand";
                        break;
                    case "Offhand":
                        inventoryId = "OffHand";
                        break;
                    case "Flask":
                        inventoryId = $"Flask{jobj.Value<int>("x") + 1}";
                        break;
                }

                if (EnumsNET.Enums.TryParse(inventoryId, out ItemSlot slot))
                {
                    var item = AddItem(jobj, slot);
                    AddSkillsToSlot(item.DeserializeSocketedSkills(_skillDefinitions), slot);
                    item.SetJsonBase();
                }
            }
        }

        private void DeserializeSkills(JObject itemData)
        {
            if (!itemData.TryGetValue("skills", out var skillJson))
                return;

            var skillList = skillJson.ToObject<IReadOnlyList<Skill>>();
            foreach (var (slot, group) in skillList.GroupBy(s => s.ItemSlot))
            {
                AddSkillsToSlot(group, slot);
            }
        }

        public string ToJsonString()
        {
            var items = new JArray();
            foreach (var item in Equip)
            {
                var jItem = item.JsonBase;
                jItem["inventoryId"] = item.Slot.ToString();
                items.Add(jItem);
            }

            var skillEnumerable = Skills.Flatten()
                .Where(s => s.ItemSlot != ItemSlot.Unequipable);
            var skills = JArray.FromObject(skillEnumerable);

            var jObj = new JObject
            {
                {"items", items},
                {"skills", skills},
            };
            return jObj.ToString(Formatting.None);
        }

        public void Dispose()
        {
            foreach (var item in Equip)
            {
                item.PropertyChanged -= SlottedItemOnPropertyChanged;
            }
            Equip.CollectionChanged -= OnCollectionChanged;
            Skills.CollectionChanged -= OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
            => OnItemDataChanged();

        private void SlottedItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Item.JsonBase))
            {
                OnItemDataChanged();
            }
        }

        private void OnItemDataChanged()
            => ItemDataChanged?.Invoke(this, EventArgs.Empty);

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

        private Item AddItem(JObject val, ItemSlot islot)
        {
            var item = new Item(_equipmentData, val, islot);
            Equip.Add(item);
            item.PropertyChanged += SlottedItemOnPropertyChanged;
            return item;
        }


        public class Attribute : Notifier
        {
            public static readonly Regex Backreplace = new Regex("#");

            private readonly List<float> _value;

            public string Group { get; }

            public string TextAttribute { get; }

            public string ValuedAttribute
            {
                get { return _value.Aggregate(TextAttribute, (current, f) => Backreplace.Replace(current, f + "", 1)); }
            }

            public Attribute(string s, IEnumerable<float> val, string grp)
            {
                TextAttribute = s;
                _value = new List<float>(val);
                Group = grp;
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