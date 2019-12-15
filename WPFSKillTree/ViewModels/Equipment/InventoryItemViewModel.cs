using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Model.Items;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for draggable items in the inventory. This is also a drop target.
    /// </summary>
    public sealed class InventoryItemViewModel : DraggableItemViewModel, IDropTarget, IDisposable
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        public ushort? Socket { get; }

        // the item is delegated to this view model's slot in ItemAttributes
        public override Item? Item
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

        private bool _isCurrent;
        public bool IsCurrent
        {
            get => _isCurrent;
            set => SetProperty(ref _isCurrent, value);
        }

        /// <summary>
        /// Gets or sets the path to the image that should be shown if Item is null.
        /// </summary>
        public string EmptyBackgroundImagePath { get; }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Link;
        public override DragDropEffects DropOnStashEffect => DragDropEffects.Copy;

        public ICommand EditSocketedGemsCommand { get; }

        public InventoryItemViewModel(
            IExtendedDialogCoordinator dialogCoordinator, ItemAttributes itemAttributes, ItemSlot slot, ushort? socket,
            string emptyBackgroundImagePath)
        {
            _dialogCoordinator = dialogCoordinator;
            _itemAttributes = itemAttributes;
            _slot = slot;
            Socket = socket;
            EmptyBackgroundImagePath = emptyBackgroundImagePath;
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
            if (dropInfo.Data is DraggableItemViewModel draggedItem
                && _itemAttributes.CanEquip(draggedItem.Item, _slot, Socket)
                && draggedItem != this) // can't drop onto itself
            {
                dropInfo.Effects = draggedItem.DropOnInventoryEffect;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var draggedItem = (DraggableItemViewModel) dropInfo.Data;
            var dropEffects = dropInfo.Effects;

            var oldItem = Item;
            var newItem = dropEffects == DragDropEffects.Copy
                ? new Item(draggedItem.Item!) : draggedItem.Item;

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
