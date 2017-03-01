using System.Windows;
using System.Windows.Media;
using GongSolutions.Wpf.DragDrop;

namespace POESKillTree.ViewModels.Equipment
{
    public class BinDropTargetAdorner : DropTargetAdorner
    {
        public BinDropTargetAdorner(UIElement adornedElement, DropInfo dropInfo)
            : base(adornedElement, dropInfo)
        {
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var dropInfo = DropInfo;
            var visualTargetItem = dropInfo.VisualTargetItem;
            if (visualTargetItem != null)
            {
                var rect = new Rect(visualTargetItem.TranslatePoint(new Point(), AdornedElement),
                    VisualTreeHelper.GetDescendantBounds(visualTargetItem).Size);
                rect.X += 2;
                rect.Y += 2;
                rect.Width -= 4;
                rect.Height -= 4;
                drawingContext.DrawRectangle(Pen.Brush, null, rect);
            }
        }
    }

    public class BinViewModel : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            if (draggedItem != null)
            {
                dropInfo.Effects = draggedItem.DropOnBinEffect;
                dropInfo.DropTargetAdorner = typeof(BinDropTargetAdorner);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var draggedItem = (DraggableItemViewModel) dropInfo.Data;
                draggedItem.Item = null;
            }
        }
    }
}
