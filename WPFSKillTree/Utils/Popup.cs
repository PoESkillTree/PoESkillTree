using System.Threading.Tasks;
using System.Windows;
using POESKillTree.Localization;
using POESKillTree.Views;

namespace POESKillTree.Utils
{
    public static class Popup
    {
        // Displays generic Yes/No question message box.
        public static Task<MessageBoxResult> Ask(string message, string title = null, MessageBoxImage icon = MessageBoxImage.Question)
        {
            return MetroMessageBox.Show(GetActiveWindow(), message, null, title ?? L10n.Message("Confirmation"), MessageBoxButton.YesNo, icon, MessageBoxResult.No);
        }

        // Displays generic error message box with optional details.
        public static Task Error(string message, string details = null)
        {
            return MetroMessageBox.Show(GetActiveWindow(), message, details, L10n.Message("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Returns active window.
        private static Window GetActiveWindow()
        {
            var windows = Application.Current.Windows;
            if (windows.Count > 1)
            {
                foreach (Window win in windows)
                    if (win.IsActive) return win;

                return windows[windows.Count - 1];
            }

            return Application.Current.MainWindow;
        }

        // Displays generic information message box with optional details.
        public static Task Info(string message, string details = null, string title = null)
        {
            return MetroMessageBox.Show(GetActiveWindow(), message, details, title ?? L10n.Message("Information"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Displays generic warning message box with optional details.
        public static Task Warning(string message, string details = null, string title = null)
        {
            return MetroMessageBox.Show(GetActiveWindow(), message, details, title ?? L10n.Message("Warning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
