using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using OldItem = POESKillTree.Model.Items.Item;

namespace POESKillTree.Model
{
    public class ObservableItemCollectionConverter
    {
        private ObservableCollection<OldItem> _oldCollection;

        private readonly Dictionary<ItemSlot, OldItem> _itemSlotToOldItem =
            new Dictionary<ItemSlot, OldItem>();

        private readonly Dictionary<ItemSlot, IReadOnlyList<Skill>> _itemSlotToSkills =
            new Dictionary<ItemSlot, IReadOnlyList<Skill>>();

        public ObservableCollection<(Item, ItemSlot)> Items { get; } =
            new ObservableCollection<(Item, ItemSlot)>();

        public ObservableCollection<IReadOnlyList<Skill>> Skills { get; } =
            new ObservableCollection<IReadOnlyList<Skill>> { new[] { Skill.Default, } };

        public void ConvertFrom(ObservableCollection<OldItem> oldCollection)
        {
            if (_oldCollection != null)
            {
                _oldCollection.CollectionChanged -= OldCollectionChanged;
            }
            _oldCollection = oldCollection;
            Reset();
            _oldCollection.CollectionChanged += OldCollectionChanged;
        }

        private void OldCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                Reset();
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

        private void Reset()
        {
            _itemSlotToOldItem.Values.ToList().ForEach(Remove);
            _oldCollection.ForEach(Add);
        }

        private void Add(OldItem oldItem)
        {
            var (item, slot) = Convert(oldItem);
            var skills = ModelConverter.ConvertSkills(oldItem);

            oldItem.PropertyChanged += OldItemOnPropertyChanged;

            _itemSlotToOldItem[slot] = oldItem;
            _itemSlotToSkills[slot] = skills;
            Items.Add((item, slot));
            Skills.Add(skills);
        }

        private void Remove(OldItem oldItem)
        {
            var (item, slot) = Convert(oldItem);
            var skills = _itemSlotToSkills[slot];

            oldItem.PropertyChanged -= OldItemOnPropertyChanged;

            _itemSlotToOldItem.Remove(slot);
            _itemSlotToSkills.Remove(slot);
            Items.Remove((item, slot));
            Skills.Remove(skills);
        }

        private static (Item, ItemSlot) Convert(OldItem oldItem)
            => (ModelConverter.Convert(oldItem), oldItem.Slot);

        private void OldItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var oldItem = (OldItem) sender;
            switch (e.PropertyName)
            {
                case nameof(OldItem.SocketedSkills):
                    OldItemOnSocketedSkillsChanged(oldItem);
                    break;
                case nameof(OldItem.IsEnabled):
                    OldItemOnIsEnabledChanged(oldItem);
                    break;
            }
        }

        private void OldItemOnSocketedSkillsChanged(OldItem oldItem)
        {
            var slot = oldItem.Slot;
            var oldSkills = _itemSlotToSkills[slot];
            var newSkills = ModelConverter.ConvertSkills(oldItem);

            _itemSlotToSkills[slot] = newSkills;
            Skills.Remove(oldSkills);
            Skills.Add(newSkills);
        }

        private void OldItemOnIsEnabledChanged(OldItem oldItem)
        {
            var itemTuple = Convert(oldItem);
            Items.Remove(itemTuple);
            Items.Add(itemTuple);
        }
    }
}