using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;
using POESKillTree.Model.Items;
using Item = PoESkillTree.GameModel.Items.Item;
using OldItem = POESKillTree.Model.Items.Item;

namespace POESKillTree.Model
{
    public class ObservableItemCollectionConverter
    {
        private ObservableCollection<OldItem> _itemAttributesEquip;
        private ObservableCollection<IReadOnlyList<Skill>> _itemAttributesSkills;

        private readonly Dictionary<ItemSlot, OldItem> _itemSlotToOldItem =
            new Dictionary<ItemSlot, OldItem>();

        public ObservableCollection<(Item, ItemSlot)> Items { get; } =
            new ObservableCollection<(Item, ItemSlot)>();

        public ObservableCollection<IReadOnlyList<Skill>> Skills { get; } =
            new ObservableCollection<IReadOnlyList<Skill>>();

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

        private void ItemAttributesEquipOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetItems();
                return;
            }
            if (args.NewItems is IList newItems)
            {
                newItems.Cast<OldItem>().ForEach(Add);
            }
            if (args.OldItems is IList oldItems)
            {
                oldItems.Cast<OldItem>().ForEach(Remove);
            }
        }

        private void ItemAttributesSkillsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetSkills();
                return;
            }
            if (args.NewItems is IList newItems)
            {
                Skills.AddRange(newItems.Cast<IReadOnlyList<Skill>>());
            }
            if (args.OldItems is IList oldItems)
            {
                oldItems.Cast<IReadOnlyList<Skill>>().ForEach(ss => Skills.Remove(ss));
            }
        }

        private void ResetItems()
        {
            _itemSlotToOldItem.Values.ToList().ForEach(Remove);
            _itemAttributesEquip.ForEach(Add);
        }

        private void ResetSkills()
        {
            Skills.ToList().ForEach(ss => Skills.Remove(ss));
            Skills.AddRange(_itemAttributesSkills);
        }

        private void Add(OldItem oldItem)
        {
            var (item, slot) = Convert(oldItem);

            oldItem.PropertyChanged += OldItemOnPropertyChanged;

            _itemSlotToOldItem[slot] = oldItem;
            Items.Add((item, slot));
        }

        private void Remove(OldItem oldItem)
        {
            var (item, slot) = Convert(oldItem);

            oldItem.PropertyChanged -= OldItemOnPropertyChanged;

            _itemSlotToOldItem.Remove(slot);
            Items.Remove((item, slot));
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