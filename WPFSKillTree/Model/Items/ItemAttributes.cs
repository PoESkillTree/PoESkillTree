using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
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

        public IReadOnlyList<Gem> GetGemsInSlot(ItemSlot slot) =>
            Gems.FirstOrDefault(gs => gs.First().ItemSlot == slot) ?? Array.Empty<Gem>();

        public void SetGemsInSlot(IReadOnlyList<Gem> value, ItemSlot slot)
        {
            if (value.Any(s => s.ItemSlot != slot))
                throw new ArgumentException("Gems for a slot must all have that slot as ItemSlot", nameof(value));

            var oldValue = GetGemsInSlot(slot);
            if (oldValue.Any() && value.Any())
            {
                Gems.RemoveAndAdd(oldValue, value);
            }
            else if (value.Any())
            {
                Gems.Add(value);
            }
            else if (oldValue.Any())
            {
                Gems.Remove(oldValue);
            }
        }

        public IReadOnlyList<Skill> GetSkillsInSlot(ItemSlot slot) =>
            Skills.FirstOrDefault(ss => ss.First().ItemSlot == slot) ?? Array.Empty<Skill>();

        #endregion

        public ObservableSet<Item> Equip { get; } = new ObservableSet<Item>();
        
        public ObservableSet<IReadOnlyList<Gem>> Gems { get; } = new ObservableSet<IReadOnlyList<Gem>>();
        public ObservableSet<IReadOnlyList<Skill>> Skills { get; } = new ObservableSet<IReadOnlyList<Skill>>();
        public SkillEnabler SkillEnabler { get; } = new SkillEnabler();

        private readonly EquipmentData _equipmentData;
        private readonly SkillDefinitions _skillDefinitions;

        public event EventHandler? ItemDataChanged;
        public event Action<(ItemSlot, ushort?)>? ItemChanged;

        public ItemAttributes(EquipmentData equipmentData, SkillDefinitions skillDefinitions, string? itemData = null)
        {
            _equipmentData = equipmentData;
            _skillDefinitions = skillDefinitions;
            Equip.CollectionChanged += OnCollectionChanged;
            Gems.CollectionChanged += OnCollectionChanged;
            SkillEnabler.JsonRepresentationChanged += OnCollectionChanged;
            Skills.Add(new[] {Skill.Default});
            Skills.CollectionChanged += SkillsOnCollectionChanged;

            if (!string.IsNullOrEmpty(itemData))
            {
                var jObject = JObject.Parse(itemData);
                DeserializeItemsWithGems(jObject);
                DeserializeGems(jObject);
                DeserializeEnabledSkills(jObject);
            }
        }

        public void DeserializeItemsWithGems(JObject itemData, bool addItems = true, bool addGems = true)
        {
            if (!itemData.TryGetValue("items", out var itemsJson))
                return;

            foreach (var itemJson in itemsJson.Values<JObject>())
            {
                DeserializeItemWithSkills(itemJson, addItems, addGems);
            }
        }

        private void DeserializeItemWithSkills(JObject itemJson, bool addItem, bool addGems)
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
                if (addGems)
                {
                    SetGemsInSlot(item.DeserializeSocketedGems(_skillDefinitions, itemJson), slot);
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

        private void DeserializeGems(JObject itemData)
        {
            IEnumerable<Gem> gems;
            if (itemData.TryGetValue("skills", out var skillsJson))
            {
                gems = skillsJson.Select(DeserializeGemFromSkill);
            }
            else if (itemData.TryGetValue("gems", out var gemsJson))
            {
                gems = gemsJson.ToObject<IReadOnlyList<Gem>>()!;
            }
            else
            {
                return;
            }

            foreach (var (slot, group) in gems.GroupBy(g => g.ItemSlot))
            {
                SetGemsInSlot(GetGemsInSlot(slot).Concat(group).ToList(), slot);
            }
        }

        private static Gem DeserializeGemFromSkill(JToken skillJson) =>
            new Gem(skillJson.Value<string>("Id"),
                skillJson.Value<int>("Level"),
                skillJson.Value<int>("Quality"),
                (ItemSlot) skillJson.Value<int>("ItemSlot"),
                skillJson.Value<int>("SocketIndex"),
                skillJson.Value<int>("GemGroup"),
                skillJson.Value<bool>("IsEnabled"));

        private void DeserializeEnabledSkills(JObject itemData)
        {
            if (itemData.TryGetValue("enabledSkills", out var json))
            {
                SkillEnabler.FromJson(json);
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

            var gems = JArray.FromObject(Gems.Flatten());

            var jObj = new JObject
            {
                {"items", items},
                {"gems", gems},
                {"enabledSkills", SkillEnabler.ToJsonString()},
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
            Gems.CollectionChanged -= OnCollectionChanged;
            SkillEnabler.JsonRepresentationChanged -= OnCollectionChanged;
            Skills.CollectionChanged -= SkillsOnCollectionChanged;
        }

        private void OnCollectionChanged(object? sender, EventArgs args)
            => OnItemDataChanged();

        private void SkillsOnCollectionChanged(object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            SkillEnabler.Store(args.AddedItems, args.RemovedItems);
        }

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