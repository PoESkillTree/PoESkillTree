using System;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using PoESkillTree.Utils;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Model.Items;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for Items that can be dragged.
    /// </summary>
    public abstract class DraggableItemViewModel : Notifier, IDragSource
    {
        /// <summary>
        /// Gets or sets the item this view models shows.
        /// </summary>
        public abstract Item Item { get; set; }

        private bool _isDragged;
        /// <summary>
        /// Gets or sets whether the view model is currently being dragged.
        /// </summary>
        public bool IsDragged
        {
            get => _isDragged;
            private set => SetProperty(ref _isDragged, value);
        }

        private Point _dragMouseAnchorPoint = new Point(0, 0);
        /// <summary>
        /// Gets or sets the point where the DragAdorner (the item image) should be located relative to the cursor.
        /// The point's values are percentage of the DragAdorner's size.
        /// This is set so the cursor appears on the image at the same position as where it was when the drag started.
        /// </summary>
        public Point DragMouseAnchorPoint
        {
            get => _dragMouseAnchorPoint;
            private set => SetProperty(ref _dragMouseAnchorPoint, value);
        }

        // effects for dropping in different locations
        // Move (not inside stash): target Item will be set to source Item (if applicable), source Item will be set to null
        // Copy: target Item will be set to a copy of source Item
        // Link: source and target Items will be swapped
        // None: drop is not possible
        public virtual DragDropEffects DropOnInventoryEffect => DragDropEffects.Move;
        public virtual DragDropEffects DropOnStashEffect => DragDropEffects.Move;
        public DragDropEffects DropOnBinEffect => DragDropEffects.Move;
        private DragDropEffects AllowedEffects => DropOnInventoryEffect | DropOnStashEffect | DropOnBinEffect;

        public ICommand DeleteCommand { get; }

        protected DraggableItemViewModel()
        {
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        private void Delete()
        {
            Item = null;
        }

        private bool CanDelete()
            => Item != null;

        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Data = this;
            var image = Item.Image.ImageSource.Result;
            DragMouseAnchorPoint = new Point(
                dragInfo.PositionInDraggedItem.X / image.Width, 
                dragInfo.PositionInDraggedItem.Y / image.Height
            );
            dragInfo.Effects = AllowedEffects;
            IsDragged = true;
        }

        public bool CanStartDrag(IDragInfo dragInfo)
            => Item != null;

        public void Dropped(IDropInfo dropInfo)
        {
            IsDragged = false;
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
        }

        public void DragCancelled()
        {
            IsDragged = false;
        }

        public bool TryCatchOccurredException(Exception exception)
            => false;
    }
}