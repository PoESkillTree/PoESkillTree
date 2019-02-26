using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils;
using POESKillTree.Model.Items;
using POESKillTree.Utils;
using Item = PoESkillTree.GameModel.Items.Item;
using OldItem = POESKillTree.Model.Items.Item;

namespace POESKillTree.Model
{
    public class ObservableItemCollectionConverter
    {
        private ObservableSet<OldItem> _itemAttributesEquip;
        private ObservableSet<IReadOnlyList<Skill>> _itemAttributesSkills;

        private readonly Dictionary<ItemSlot, OldItem> _itemSlotToOldItem =
            new Dictionary<ItemSlot, OldItem>();

        public ObservableSet<(Item, ItemSlot)> Items { get; } = new ObservableSet<(Item, ItemSlot)>();

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
        {
            Remove(args.RemovedItems);
            Add(args.AddedItems);
        }

        private void ItemAttributesSkillsOnCollectionChanged(
            object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
        {
            Skills.ExceptWith(args.RemovedItems);
            Skills.UnionWith(args.AddedItems);
        }

        private void ResetItems()
        {
            Remove(_itemSlotToOldItem.Values.ToList());
            Add(_itemAttributesEquip);
        }

        private void ResetSkills()
        {
            Skills.Clear();
            Skills.UnionWith(_itemAttributesSkills);
        }

        private void Add(IEnumerable<OldItem> oldItems)
        {
            var toAdd = new List<(Item, ItemSlot)>();
            foreach (var oldItem in oldItems)
            {
                var (item, slot) = Convert(oldItem);

                oldItem.PropertyChanged += OldItemOnPropertyChanged;

                _itemSlotToOldItem[slot] = oldItem;
                toAdd.Add((item, slot));
            }
            Items.UnionWith(toAdd);
        }

        private void Remove(IEnumerable<OldItem> oldItems)
        {
            var toRemove = new List<(Item, ItemSlot)>();
            foreach (var oldItem in oldItems)
            {
                var (item, slot) = Convert(oldItem);

                oldItem.PropertyChanged -= OldItemOnPropertyChanged;

                _itemSlotToOldItem.Remove(slot);
                toRemove.Add((item, slot));
            }
            Items.ExceptWith(toRemove);
        }

        private static (Item, ItemSlot) Convert(OldItem oldItem)
            => (ModelConverter.Convert(oldItem), oldItem.Slot);

        private void OldItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OldItem.IsEnabled))
            {
                OldItemOnIsEnabledChanged((OldItem) sender);
            }
        }

        private void OldItemOnIsEnabledChanged(OldItem oldItem)
        {
            var itemTuple = Convert(oldItem);
            Items.Remove(itemTuple);
            Items.Add(itemTuple);
        }
    }
}