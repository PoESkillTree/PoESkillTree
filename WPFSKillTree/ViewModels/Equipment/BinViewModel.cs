using System.Windows;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;

namespace PoESkillTree.ViewModels.Equipment
{
    /// <summary>
    /// View model for the bin. Items can be dropped onto it to remove them.
    /// </summary>
    public class BinViewModel : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            // only DraggableItemViewModels can be dropped, they can always be dropped
            if (dropInfo.Data is DraggableItemViewModel draggedItem)
            {
                dropInfo.Effects = draggedItem.DropOnBinEffect;
                dropInfo.DropTargetAdorner = typeof(BinDropTargetAdorner);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                // remove the item from its view model on move
                var draggedItem = (DraggableItemViewModel) dropInfo.Data;
                draggedItem.Item = null;
            }
        }

        /// <summary>
        /// The DropTargetAdorner for the bin. A simple rectangle covering the whole view except for its border.
        /// </summary>
        private class BinDropTargetAdorner : DropTargetAdorner
        {
            public BinDropTargetAdorner(UIElement adornedElement, DropInfo dropInfo)
                : base(adornedElement, dropInfo)
            {
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                var rect = new Rect(
                    DropInfo.VisualTargetItem.TranslatePoint(new Point(), AdornedElement),
                    VisualTreeHelper.GetDescendantBounds(DropInfo.VisualTargetItem).Size
                );
                rect.X += 2;
                rect.Y += 2;
                rect.Width -= 4;
                rect.Height -= 4;
                drawingContext.DrawRectangle(Pen.Brush, null, rect);
            }
        }
    }
}
