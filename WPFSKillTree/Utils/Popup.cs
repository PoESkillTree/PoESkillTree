using System;
using System.Text;
using System.Windows;
using POESKillTree.Localization;
using POESKillTree.Views;

namespace POESKillTree.Utils
{
    class Popup
    {
        // Displays generic Yes/No question message box.
        public static MessageBoxResult Ask(string message, MessageBoxImage icon = MessageBoxImage.Question)
        {
            return MetroMessageBox.Show(GetActiveWindow(), message, L10n.Message("Question"), MessageBoxButton.YesNo, icon);
        }

        // Displays generic error message box with optional details.
        public static void Error(string message, string details = null)
        {
            MetroMessageBox.Show(GetActiveWindow(), Format(message, details), L10n.Message("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Formats message with optional details.
        static string Format(string message, string details)
        {
            if (!string.IsNullOrEmpty(details))
                message += "\n\n" + L10n.Message("Details:") + "\n" + details;

            return message;
        }

        // Returns active window.
        static Window GetActiveWindow()
        {
            if (App.Current.Windows.Count > 1)
            {
                foreach (Window win in App.Current.Windows)
                    if (win.IsActive) return win;

                return App.Current.Windows[App.Current.Windows.Count - 1];
            }

            return App.Current.MainWindow;
        }

        // Displays generic information message box with optional details.
        public static void Info(string message, string details = null)
        {
            MetroMessageBox.Show(GetActiveWindow(), Format(message, details), L10n.Message("Information"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Displays generic warning message box with optional details.
        public static void Warning(string message, string details = null)
        {
            MetroMessageBox.Show(GetActiveWindow(), Format(message, details), L10n.Message("Warning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
