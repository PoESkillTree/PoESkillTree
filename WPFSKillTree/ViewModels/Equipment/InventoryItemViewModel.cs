using System.Windows;
using GongSolutions.Wpf.DragDrop;
using JetBrains.Annotations;
using PoESkillTree.GameModel.Items;
using POESKillTree.Model.Items;
using Item = POESKillTree.Model.Items.Item;

namespace POESKillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for draggable items in the inventory. This is also a drop target.
    /// </summary>
    public class InventoryItemViewModel : DraggableItemViewModel, IDropTarget
    {
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        // the item is delegated to this view model's slot in ItemAttributes
        public override Item Item
        {
            get { return _itemAttributes.GetItemInSlot(_slot); }
            set { _itemAttributes.SetItemInSlot(value, _slot); }
        }

        private string _emptyBackgroundImagePath;
        /// <summary>
        /// Gets or sets the path to the image that should be shown if Item is null.
        /// </summary>
        // used in styles, Visual Studio/Resharper somehow doesn't recognize that
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string EmptyBackgroundImagePath
        {
            get { return _emptyBackgroundImagePath; }
            set { SetProperty(ref _emptyBackgroundImagePath, value); }
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Link;
        public override DragDropEffects DropOnStashEffect => DragDropEffects.Copy;

        public InventoryItemViewModel(IExtendedDialogCoordinator dialogCoordinator,
            ItemAttributes itemAttributes, ItemSlot slot)
            : base(dialogCoordinator)
        {
            _itemAttributes = itemAttributes;
            _slot = slot;

            // Item changes when the slotted item in ItemAttribute changes as they are the same
            _itemAttributes.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == slot.ToString())
                {
                    OnPropertyChanged(nameof(Item));
                }
            };
        }

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
                var item = draggedItem.Item;
                draggedItem.Item = Item;
                Item = item;
            }
        }
    }
}
