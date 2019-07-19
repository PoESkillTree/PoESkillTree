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
            => ChangeItems(args.RemovedItems, args.AddedItems);

        private void ItemAttributesSkillsOnCollectionChanged(
            object sender, CollectionChangedEventArgs<IReadOnlyList<Skill>> args)
            => Skills.ExceptAndUnionWith(args.RemovedItems, args.AddedItems);

        private void ResetItems()
            => ChangeItems(_itemSlotToOldItem.Values.ToList(), _itemAttributesEquip);

        private void ResetSkills()
            => Skills.ExceptAndUnionWith(Skills.ToList(), _itemAttributesSkills);

        private void ChangeItems(IEnumerable<OldItem> oldToRemove, IEnumerable<OldItem> oldToAdd)
        {
            var toRemove = new List<(Item, ItemSlot)>();
            foreach (var oldItem in oldToRemove)
            {
                var tuple = Convert(oldItem);
                oldItem.PropertyChanged -= OldItemOnPropertyChanged;
                _itemSlotToOldItem.Remove(tuple.slot);
                toRemove.Add(tuple);
            }
            var toAdd = new List<(Item, ItemSlot)>();
            foreach (var oldItem in oldToAdd)
            {
                var tuple = Convert(oldItem);
                oldItem.PropertyChanged += OldItemOnPropertyChanged;
                _itemSlotToOldItem[tuple.slot] = oldItem;
                toAdd.Add(tuple);
            }
            Items.ExceptAndUnionWith(toRemove, toAdd);
        }

        private static (Item item, ItemSlot slot) Convert(OldItem oldItem)
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