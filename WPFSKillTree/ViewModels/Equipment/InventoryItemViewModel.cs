using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using PoESkillTree.GameModel.Items;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using Item = POESKillTree.Model.Items.Item;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for draggable items in the inventory. This is also a drop target.
    /// </summary>
    public class InventoryItemViewModel : DraggableItemViewModel, IDropTarget
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        // the item is delegated to this view model's slot in ItemAttributes
        public override Item Item
        {
            get => _itemAttributes.GetItemInSlot(_slot);
            set => _itemAttributes.SetItemInSlot(value, _slot);
        }

        private string _emptyBackgroundImagePath;
        /// <summary>
        /// Gets or sets the path to the image that should be shown if Item is null.
        /// </summary>
        public string EmptyBackgroundImagePath
        {
            get => _emptyBackgroundImagePath;
            set => SetProperty(ref _emptyBackgroundImagePath, value);
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Link;
        public override DragDropEffects DropOnStashEffect => DragDropEffects.Copy;

        public ICommand EditSocketedGemsCommand { get; }

        public InventoryItemViewModel(
            IExtendedDialogCoordinator dialogCoordinator, ItemAttributes itemAttributes, ItemSlot slot)
        {
            _dialogCoordinator = dialogCoordinator;
            _itemAttributes = itemAttributes;
            _slot = slot;

            EditSocketedGemsCommand = new AsyncRelayCommand(EditSocketedGemsAsync, CanEditSocketedGems);

            // Item changes when the slotted item in ItemAttribute changes as they are the same
            _itemAttributes.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == slot.ToString())
                {
                    OnPropertyChanged(nameof(Item));
                }
            };
        }

        private async Task EditSocketedGemsAsync()
            => await _dialogCoordinator.EditSocketedGemsAsync(this, _itemAttributes, _slot);

        private bool CanEditSocketedGems()
            => !_slot.IsFlask();

        public void DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;

            if (draggedItem == null
                || !_itemAttributes.CanEquip(draggedItem.Item, _slot)
                || draggedItem == this) // can't drop onto itself
            {
                return;
            }
            dropInfo.Effects = draggedItem.DropOnInventoryEffect;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var draggedItem = (DraggableItemViewModel) dropInfo.Data;

            if (dropInfo.Effects == DragDropEffects.Move)
            {
                Item = draggedItem.Item;
                draggedItem.Item = null;
            }
            else if (dropInfo.Effects == DragDropEffects.Copy)
            {
                Item = new Item(draggedItem.Item);
            }
            else if (dropInfo.Effects == DragDropEffects.Link)
            {
                // Link = Swap
                var newItem = draggedItem.Item;
                var oldItem = Item;
                Item = null;
                draggedItem.Item = oldItem;
                Item = newItem;
            }
        }
    }
}
