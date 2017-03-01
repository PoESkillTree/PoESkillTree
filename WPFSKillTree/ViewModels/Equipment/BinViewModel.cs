using System.Windows;
using GongSolutions.Wpf.DragDrop;

namespace POESKillTree.ViewModels.Equipment
{
    public class BinViewModel : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            var draggedItem = dropInfo.Data as DraggableItemViewModel;
            dropInfo.Effects = draggedItem?.DropOnBinEffect ?? DragDropEffects.None;
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
