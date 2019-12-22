using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Utils;

namespace PoESkillTree.Model.Items
{
    public sealed class ItemAttributes : IDisposable
    {
        #region slotted items

        private Item? MainHand => GetItemInSlot(ItemSlot.MainHand, null);

        private Item? OffHand => GetItemInSlot(ItemSlot.OffHand, null);

        public Item? GetItemInSlot(ItemSlot slot, ushort? socket)
            => Equip.FirstOrDefault(i => i.Slot == slot && i.Socket == socket);

        public void RemoveItem(Item item) => SetItemInSlot(null, item.Slot, item.Socket);

        public void SetItemInSlot(Item? value, ItemSlot slot, ushort? socket)
        {
            if (!CanEquip(value, slot, socket))
                return;
            
            var old = GetItemInSlot(slot, socket);
            if (value is null)
            {
                if (old != null)
                {
                    Equip.Remove(old);
                }
            }
            else
            {
                value.Slot = slot;
                value.Socket = socket;
                if (old is null)
                {
                    Equip.Add(value);
                }
                else
                {
                    Equip.RemoveAndAdd(old, value);
                }
            }

            if (old != null)
            {
                old.Slot = ItemSlot.Unequipable;
                old.Socket = null;
                old.PropertyChanged -= SlottedItemOnPropertyChanged;
            }
            if (value != null)
            {
                value.PropertyChanged += SlottedItemOnPropertyChanged;
            }

            OnItemChanged(slot, socket);
        }

        public bool CanEquip(Item? item, ItemSlot slot, ushort? socket)
        {
            if (item == null)
                return true;
            if (slot == ItemSlot.Unequipable)
                return false;

            if (socket.HasValue)
            {
                if (slot == ItemSlot.SkillTree)
                {
                    return item.IsJewel;
                }
                return item.ItemClass == ItemClass.AbyssJewel;
            }
            if (item.IsJewel)
            {
                return false;
            }

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

        public IReadOnlyList<Skill> GetSkillsInSlot(ItemSlot slot) =>
            Skills.FirstOrDefault(ss => ss.First().ItemSlot == slot) ?? Array.Empty<Skill>();

        public void RemoveSkills(IReadOnlyList<Skill> value) =>
            SetSkillsInSlot(Array.Empty<Skill>(), value.First().ItemSlot);

        public void SetSkillsInSlot(IReadOnlyList<Skill> value, ItemSlot slot)
        {
            if (value.Any(s => s.ItemSlot != slot))
                throw new ArgumentException("Skills for a slot must all have that slot as ItemSlot", nameof(value));

            var oldValue = GetSkillsInSlot(slot);
            if (oldValue.Any() && value.Any())
            {
                Skills.RemoveAndAdd(oldValue, value);
            }
            if (value.Any())
            {
                Skills.Add(value);
            }
            else if (oldValue.Any())
            {
                Skills.Remove(oldValue);
            }
        }

        private void AddSkillsToSlot(IEnumerable<Skill> skills, ItemSlot slot)
            => SetSkillsInSlot(GetSkillsInSlot(slot).Concat(skills).ToList(), slot);

        #endregion

        public ObservableSet<Item> Equip { get; } = new ObservableSet<Item>();

        public ObservableSet<IReadOnlyList<Skill>> Skills { get; } = new ObservableSet<IReadOnlyList<Skill>>();

        private readonly EquipmentData _equipmentData;
        private readonly SkillDefinitions _skillDefinitions;

        public event EventHandler? ItemDataChanged;
        public event Action<(ItemSlot, ushort?)>? ItemChanged;

        public ItemAttributes(EquipmentData equipmentData, SkillDefinitions skillDefinitions, string? itemData = null)
        {
            _equipmentData = equipmentData;
            _skillDefinitions = skillDefinitions;
            Equip.CollectionChanged += OnCollectionChanged;
            SetSkillsInSlot(new[] { Skill.Default, }, ItemSlot.Unequipable);
            Skills.CollectionChanged += OnCollectionChanged;

            if (!string.IsNullOrEmpty(itemData))
            {
                var jObject = JObject.Parse(itemData);
                DeserializeItemsWithSkills(jObject);
                DeserializeSkills(jObject);
            }
        }

        public void DeserializeItemsWithSkills(JObject itemData, bool addItems = true, bool addSkills = true)
        {
            if (!itemData.TryGetValue("items", out var itemsJson))
                return;

            foreach (var itemJson in itemsJson.Values<JObject>())
            {
                DeserializeItemWithSkills(itemJson, addItems, addSkills);
            }
        }

        private void DeserializeItemWithSkills(JObject itemJson, bool addItem, bool addSkills)
        {
            var inventoryId = itemJson.Value<string>("inventoryId");
            switch (inventoryId)
            {
                case "Weapon":
                    inventoryId = "MainHand";
                    break;
                case "Offhand":
                    inventoryId = "OffHand";
                    break;
                case "Flask":
                    inventoryId = $"Flask{itemJson.Value<int>("x") + 1}";
                    break;
            }

            if (EnumsNET.Enums.TryParse(inventoryId, out ItemSlot slot))
            {
                var item = new Item(_equipmentData, itemJson, slot);
                if (addItem)
                {
                    SetItemInSlot(item, slot, item.Socket);
                    foreach (var socketedItem in item.DeserializeSocketedItems(_equipmentData, itemJson))
                    {
                        SetItemInSlot(socketedItem, slot, socketedItem.Socket);
                    }
                }
                if (addSkills)
                {
                    SetSkillsInSlot(item.DeserializeSocketedSkills(_skillDefinitions, itemJson), slot);
                }
            }
        }

        public void DeserializePassiveTreeJewels(JObject treeImportJson)
        {
            if (!treeImportJson.TryGetValue("items", out var itemsJson) || !treeImportJson.TryGetValue("jewel_slots", out var socketsJson))
                return;
            
            var sockets = socketsJson.Values<ushort>().ToList();
            foreach (var itemJson in itemsJson.Values<JObject>())
            {
                var item = new Item(_equipmentData, itemJson, ItemSlot.SkillTree);
                SetItemInSlot(item, ItemSlot.SkillTree, sockets[item.X]);
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
                var jItem = item.GenerateJson();
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

        private void OnCollectionChanged(object sender, EventArgs args)
            => OnItemDataChanged();

        private void SlottedItemOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(Item.Socket) || args.PropertyName == nameof(Item.IsEnabled))
            {
                OnItemDataChanged();
            }
        }

        private void OnItemDataChanged()
            => ItemDataChanged?.Invoke(this, EventArgs.Empty);

        private void OnItemChanged(ItemSlot slot, ushort? socket)
            => ItemChanged?.Invoke((slot, socket));
    }
}