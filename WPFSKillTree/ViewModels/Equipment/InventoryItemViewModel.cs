using System.Windows;
using GongSolutions.Wpf.DragDrop;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;

namespace POESKillTree.ViewModels.Equipment
{
    public class InventoryItemViewModel : DraggableItemViewModel, IDropTarget
    {
        private readonly ItemAttributes _itemAttributes;
        private readonly ItemSlot _slot;

        public override Item Item
        {
            get { return _itemAttributes.GetItemInSlot(_slot); }
            set { _itemAttributes.SetItemInSlot(value, _slot); }
        }

        private string _emptyBackgroundImagePath;
        public string EmptyBackgroundImagePath
        {
            get { return _emptyBackgroundImagePath; }
            set { SetProperty(ref _emptyBackgroundImagePath, value); }
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Link;
        public override DragDropEffects DropOnStashEffect => DragDropEffects.Copy;

        public InventoryItemViewModel(IExtendedDialogCoordinator dialogCoordinator, EquipmentData equipmentData,
            ItemAttributes itemAttributes, ItemSlot slot)
            : base(dialogCoordinator, equipmentData)
        {
            _itemAttributes = itemAttributes;
            _slot = slot;

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
