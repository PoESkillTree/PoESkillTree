using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using POESKillTree.Common.ViewModels;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Equipment
{
    public abstract class DraggableItemViewModel : Notifier, IDragSource
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;
        private readonly EquipmentData _equipmentData;

        public abstract Item Item { get; set; }

        private bool _isDragged;
        public bool IsDragged
        {
            get { return _isDragged; }
            private set { SetProperty(ref _isDragged, value); }
        }

        private Thickness _dragAdornerMargin;
        public Thickness DragAdornerMargin
        {
            get { return _dragAdornerMargin; }
            private set { SetProperty(ref _dragAdornerMargin, value); }
        }

        public virtual DragDropEffects DropOnInventoryEffect => DragDropEffects.Move;
        public virtual DragDropEffects DropOnStashEffect => DragDropEffects.Move;
        public DragDropEffects DropOnBinEffect => DragDropEffects.Move;
        private DragDropEffects AllowedEffects => DropOnInventoryEffect | DropOnStashEffect | DropOnBinEffect;

        public ICommand DeleteCommand { get; }
        public ICommand EditSocketedGemsCommand { get; }

        protected DraggableItemViewModel(IExtendedDialogCoordinator dialogCoordinator, EquipmentData equipmentData)
        {
            _dialogCoordinator = dialogCoordinator;
            _equipmentData = equipmentData;

            DeleteCommand = new RelayCommand(Delete, CanDelete);
            EditSocketedGemsCommand = new AsyncRelayCommand(EditSocketedGemsAsync, CanEditSocketedGems);
        }

        private void Delete()
        {
            Item = null;
        }

        private bool CanDelete()
            => Item != null;

        private async Task EditSocketedGemsAsync()
        {
            await _dialogCoordinator.EditSocketedGemsAsync(this, _equipmentData.ItemImageService, Item);
        }

        private bool CanEditSocketedGems()
            => Item != null && Item.BaseType.MaximumNumberOfSockets > 0;

        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Effects = AllowedEffects;
            dragInfo.Data = this;
            var visualSource = (FrameworkElement) dragInfo.VisualSourceItem;
            DragAdornerMargin = new Thickness(
                - dragInfo.PositionInDraggedItem.X + 4,
                0,
                0,
                - visualSource.ActualHeight + dragInfo.PositionInDraggedItem.Y - 4);
            IsDragged = true;
        }

        public bool CanStartDrag(IDragInfo dragInfo)
            => Item != null;

        public void Dropped(IDropInfo dropInfo)
        {
            IsDragged = false;
        }

        public void DragCancelled()
        {
            IsDragged = false;
        }

        public bool TryCatchOccurredException(Exception exception)
            => false;
    }
}