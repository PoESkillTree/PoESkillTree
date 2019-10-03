using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Utils;
using PoESkillTree.Model.Items;
using Item = PoESkillTree.Engine.GameModel.Items.Item;
using OldItem = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.Model
{
    public class ObservableItemCollectionConverter
    {
        private ObservableSet<OldItem> _itemAttributesEquip;
        private ObservableSet<IReadOnlyList<Skill>> _itemAttributesSkills;

        private readonly Dictionary<(ItemSlot, ushort?), OldItem> _oldItems =
            new Dictionary<(ItemSlot, ushort?), OldItem>();

        public ObservableSet<(Item, ItemSlot)> Equipment { get; } = new ObservableSet<(Item, ItemSlot)>();

        public ObservableSet<(Item, ItemSlot, ushort, JewelRadius)> Jewels { get; } =
            new ObservableSet<(Item, ItemSlot, ushort, JewelRadius)>();

        public ObservableSet<IReadOnlyList<Skill>> Skills { get; } = new ObservableSet<IReadOnlyList<Skill>>();

        public void ConvertFrom(ItemAttributes itemAttributes)
        {
            if (_itemAttributesEquip != null)
                _itemAttributesEquip.CollectionChanged -= ItemAttributesEquipOnCollectionChanged;
            if (_itemAttributesSkills != null)
                _itemAttributesSkills.CollectionChanged -= ItemAttributesSkillsOnCollectionChanged;
            _itemAttributesEquip = itemAttributes.Equip;
            _itemAttributesSkills = itemAttributes.Skills;

            ResetItems();
            ResetSkills();

            _itemAttributesEquip.CollectionChanged += ItemAttributesEquipOnCollectionChanged;
            _itemAttributesSkills.CollectionChanged += ItemAttributesSkillsOnCollectionChanged;
        }

        private void ItemAttributesEquipOnCollectionChanged(object sender, CollectionChangedEventArgs<OldItem> args)
            => ChangeItems(args.RemovedItems, args.AddedItems);

        private void ItemAttributesSkillsOnCollectionChanged(
            object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
            => Skills.ExceptAndUnionWith(args.RemovedItems, args.AddedItems);

        private void ResetItems()
            => ChangeItems(_oldItems.Values.ToList(), _itemAttributesEquip);

        private void ResetSkills()
            => Skills.ExceptAndUnionWith(Skills.ToList(), _itemAttributesSkills);

        private void ChangeItems(IEnumerable<OldItem> oldToRemove, IEnumerable<OldItem> oldToAdd)
        {
            var equipmentToRemove = new List<(Item, ItemSlot)>();
            var jewelsToRemove = new List<(Item, ItemSlot, ushort, JewelRadius)>();
            RemoveItems(oldToRemove, equipmentToRemove, jewelsToRemove);

            var equipmentToAdd = new List<(Item, ItemSlot)>();
            var jewelsToAdd = new List<(Item, ItemSlot, ushort, JewelRadius)>();
            AddItems(oldToAdd, equipmentToAdd, jewelsToAdd);

            Equipment.ExceptAndUnionWith(equipmentToRemove, equipmentToAdd);
            Jewels.ExceptAndUnionWith(jewelsToRemove, jewelsToAdd);
        }

        private void RemoveItems(
            IEnumerable<OldItem> oldToRemove,
            List<(Item, ItemSlot)> equipmentToRemove,
            List<(Item, ItemSlot, ushort, JewelRadius)> jewelsToRemove)
        {
            foreach (var oldItem in oldToRemove)
            {
                var (item, slot, socket) = Convert(oldItem);
                oldItem.PropertyChanged -= OldItemOnPropertyChanged;
                if (socket is null)
                {
                    equipmentToRemove.Add((item, slot));
                }
                else
                {
                    jewelsToRemove.Add((item, slot, socket.Value, oldItem.JewelRadius));
                }
                _oldItems.Remove((slot, socket));
            }
        }

        private void AddItems(
            IEnumerable<OldItem> oldToAdd,
            List<(Item, ItemSlot)> equipmentToAdd,
            List<(Item, ItemSlot, ushort, JewelRadius)> jewelsToAdd)
        {
            foreach (var oldItem in oldToAdd)
            {
                var (item, slot, socket) = Convert(oldItem);
                oldItem.PropertyChanged += OldItemOnPropertyChanged;
                if (socket is null)
                {
                    equipmentToAdd.Add((item, slot));
                }
                else
                {
                    jewelsToAdd.Add((item, slot, socket.Value, oldItem.JewelRadius));
                }

                _oldItems[(slot, socket)] = oldItem;
            }
        }

        private static (Item item, ItemSlot slot, ushort? socket) Convert(OldItem oldItem)
            => (ModelConverter.Convert(oldItem), oldItem.Slot, oldItem.Socket);

        private void OldItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OldItem.IsEnabled))
            {
                OldItemOnIsEnabledChanged((OldItem) sender);
            }
        }

        private void OldItemOnIsEnabledChanged(OldItem oldItem)
        {
            var (item, slot, socket) = Convert(oldItem);
            var itemWithInvertedIsEnabled = new Item(item.BaseMetadataId, item.Name, item.Quality, item.RequiredLevel,
                item.FrameType, item.IsCorrupted, item.Modifiers, !item.IsEnabled);
            if (socket is null)
            {
                Equipment.RemoveAndAdd((itemWithInvertedIsEnabled, slot), (item, slot));
            }
            else
            {
                Jewels.RemoveAndAdd(
                    (itemWithInvertedIsEnabled, slot, socket.Value, oldItem.JewelRadius),
                    (item, slot, socket.Value, oldItem.JewelRadius));
            }
        }
    }
}