using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using JetBrains.Annotations;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Model.Items;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for draggable items in the inventory. This is also a drop target.
    /// </summary>
    public class InventoryItemViewModel : DraggableItemViewModel, IDropTarget, IDisposable
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        public ushort? Socket { get; }

        // the item is delegated to this view model's slot in ItemAttributes
        [CanBeNull]
        public override Item Item
        {
            get => _itemAttributes.GetItemInSlot(_slot, Socket);
            set => _itemAttributes.SetItemInSlot(value, _slot, Socket);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, () => ItemIsEnabled = value);
        }

        private bool ItemIsEnabled
        {
            get => Item?.IsEnabled ?? true;
            set
            {
                if (Item is Item item)
                {
                    item.IsEnabled = value;
                }
            }
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
            IExtendedDialogCoordinator dialogCoordinator, ItemAttributes itemAttributes, ItemSlot slot, ushort? socket)
        {
            _dialogCoordinator = dialogCoordinator;
            _itemAttributes = itemAttributes;
            _slot = slot;
            Socket = socket;
            _isEnabled = ItemIsEnabled;

            EditSocketedGemsCommand = new AsyncRelayCommand(EditSocketedGemsAsync, CanEditSocketedGems);

            _itemAttributes.ItemChanged += ItemAttributesOnItemChanged;
        }

        public void Dispose()
        {
            _itemAttributes.ItemChanged -= ItemAttributesOnItemChanged;
        }

        private void ItemAttributesOnItemChanged((ItemSlot, ushort?) args)
        {
            if ((_slot, Socket) == args)
            {
                ItemIsEnabled = IsEnabled;
                OnPropertyChanged(nameof(Item));
            }
        }

        private async Task EditSocketedGemsAsync()
            => await _dialogCoordinator.EditSocketedGemsAsync(this, _itemAttributes, _slot);

        private bool CanEditSocketedGems()
            => !_slot.IsFlask() && Socket is null;

        public void DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;

            if (draggedItem == null
                || !_itemAttributes.CanEquip(draggedItem.Item, _slot, Socket)
                || draggedItem == this) // can't drop onto itself
            {
                return;
            }
            dropInfo.Effects = draggedItem.DropOnInventoryEffect;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var draggedItem = (DraggableItemViewModel) dropInfo.Data;
            var dropEffects = dropInfo.Effects;

            var oldItem = Item;
            var newItem = dropEffects == DragDropEffects.Copy
                ? new Item(draggedItem.Item) : draggedItem.Item;

            if (dropEffects == DragDropEffects.Move || dropEffects == DragDropEffects.Link)
            {
                draggedItem.Item = null;
            }

            Item = newItem;

            if (dropEffects == DragDropEffects.Link)
            {
                draggedItem.Item = oldItem;
            }
        }
    }
}
