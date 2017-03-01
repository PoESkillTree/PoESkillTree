using System.Windows;
using POESKillTree.Model.Items;

namespace POESKillTree.ViewModels.Equipment
{
    public class StashItemViewModel : DraggableItemViewModel
    {
        private Item _item;
        public sealed override Item Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        private bool _highlight;
        public bool Highlight
        {
            get { return _highlight; }
            set { SetProperty(ref _highlight, value); }
        }

        public override DragDropEffects DropOnInventoryEffect => DragDropEffects.Copy;

        public StashItemViewModel(IExtendedDialogCoordinator dialogCoordinator, EquipmentData equipmentData,
            Item item)
            : base(dialogCoordinator, equipmentData)
        {
            Item = item;
        }
    }
}
