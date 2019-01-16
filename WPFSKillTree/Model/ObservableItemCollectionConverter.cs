using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;
using POESKillTree.Utils.Extensions;
using OldItem = POESKillTree.Model.Items.Item;

namespace POESKillTree.Model
{
    public class ObservableItemCollectionConverter
    {
        private ObservableCollection<OldItem> _oldCollection;

        public ObservableCollection<(Item, ItemSlot)> Collection { get; } =
            new ObservableCollection<(Item, ItemSlot)>();

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
                Add(newItems);
            }
            if (args.OldItems is IList oldItems)
            {
                Remove(oldItems);
            }
        }

        private void Reset()
        {
            Collection.Clear();
            Collection.AddRange(_oldCollection.Select(Convert));
        }

        private void Add(IEnumerable newItems)
        {
            var items = newItems.Cast<OldItem>().Select(Convert);
            Collection.AddRange(items);
        }

        private void Remove(IEnumerable oldItems)
        {
            var items = oldItems.Cast<OldItem>().Select(Convert);
            items.ForEach(i => Collection.Remove(i));
        }

        private static (Item, ItemSlot) Convert(OldItem oldItem)
            => (ModelConverter.Convert(oldItem), oldItem.Slot);
    }
}