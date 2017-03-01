using System.Windows;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.Controls
{
    /// <summary>
    /// Represents an <see cref="Item"/> of an <see cref="ItemVisualizer"/> that is currently dragged.
    /// Saves useful information and shows the item image on the cursor while dragging.
    /// <para>
    /// This class need to be disposed after the drag operation is completed for the item image overlay to
    /// be removed.
    /// </para>
    /// </summary>
    /// <remarks>For usage examples, see <see cref="Views.Equipment.InventoryView"/> or <see cref="Stash"/>.</remarks>
    public class DraggedItem
    {

        //private readonly Point _offset;

        public DraggedItem()
        {
            // DragStart is subtracted for the cursor to be on the item image at the same position as where the drag
            // was initiated.
            // The window position is subtracted because the cursor position is retrieved as an absolute value.
            // The title bar height is subtracted because the content used for the adorner does not include it.
            //var winPos = GetWindowPosition(mainWindow);
            //_offset.X = -DragStart.X - winPos.X;
            //_offset.Y = -DragStart.Y - winPos.Y - mainWindow.TitlebarHeight;
        }

        private void GiveFeedback(object sender, GiveFeedbackEventArgs giveFeedbackEventArgs)
        {
            var w32Mouse = new Win32.Point();
            Win32.GetCursorPos(ref w32Mouse);
            //_dragAdorner.OffsetLeft = w32Mouse.X + _offset.X;
            //_dragAdorner.OffsetTop = w32Mouse.Y + _offset.Y;
        }

        private static Point GetWindowPosition(Window window)
        {
            // Window.Left and Top are incorrect when maximized
            if (window.WindowState == WindowState.Maximized)
            {
                var leftField = typeof(Window).GetField("_actualLeft",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var topField = typeof(Window).GetField("_actualTop",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return new Point((double) leftField.GetValue(window), (double) topField.GetValue(window));
            }
            else
            {
                return new Point(window.Left, window.Top);
            }
        }
    }
}