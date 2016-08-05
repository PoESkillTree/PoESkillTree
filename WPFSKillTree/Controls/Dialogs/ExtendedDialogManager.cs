using System;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs.ViewModels;
using POESKillTree.Controls.Dialogs.Views;
using POESKillTree.Localization;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Controls.Dialogs
{
    public static class ExtendedDialogManager
    {
        /// <summary>
        /// Sets up the connection between view and viewModel, shows the view as a metro dialog,
        /// calls <paramref name="onShown"/> and waits for the dialog to be closed.
        /// </summary>
        public static async Task<T> ShowDialogAsync<T>(this MetroWindow window, CloseableViewModel<T> viewModel,
            BaseMetroDialog view, Action onShown = null)
        {
            view.DataContext = viewModel;

            // Undefault buttons not in the dialog as they would be pressed instead of a dialog button on enter.
            var oldDefaults = window.FindVisualChildren<Button>().Where(b => b.IsDefault).ToList();
            oldDefaults.ForEach(b => b.IsDefault = false);
            // Clear keyboard focus as they focused element is pressed instead of a dialog element on enter.
            var oldFocus = Keyboard.FocusedElement;
            Keyboard.ClearFocus();

            await window.ShowMetroDialogAsync(view);
            DialogParticipation.SetRegister(view, viewModel);
            onShown?.Invoke();

            var result = await viewModel.WaitForCloseAsync();
            await window.HideMetroDialogAsync(view);
            DialogParticipation.SetRegister(view, null);

            // Restore IsDefault and keyboard focus.
            oldDefaults.ForEach(b => b.IsDefault = true);
            Keyboard.Focus(oldFocus);

            return result;
        }

        public static Task<MessageBoxResult> ShowQuestionAsync(this MetroWindow window, string message,
            string details = null, string title = null, MessageBoxButton buttons = MessageBoxButton.YesNo,
            MessageBoxImage image = MessageBoxImage.Question)
        {
            return ShowAsync(window, message, details, title ?? L10n.Message("Confirmation"), buttons, image);
        }

        public static Task ShowErrorAsync(this MetroWindow window, string message, string details = null, string title = null)
        {
            return ShowAsync(window, message, details, title ?? L10n.Message("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static Task ShowWarningAsync(this MetroWindow window, string message, string details = null, string title = null)
        {
            return ShowAsync(window, message, details, title ?? L10n.Message("Warning"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public static Task ShowInfoAsync(this MetroWindow window, string message, string details = null, string title = null)
        {
            return ShowAsync(window, message, details, title ?? L10n.Message("Information"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static Task<string> ShowInputAsync(this MetroWindow window, string title, string message, string defaultText = "")
        {
            var settings = new MetroDialogSettings
            {
                NegativeButtonText = L10n.Message("Cancel"),
                AffirmativeButtonText = L10n.Message("OK"),
                DefaultText = defaultText
            };
            return window.ShowInputAsync(title, message, settings);
        }

        public static async Task<ProgressDialogController> ShowProgressAsync(this MetroWindow window, string title,
            string message, bool isCancelable = false)
        {
            var settings = new MetroDialogSettings
            {
                NegativeButtonText = L10n.Message("Cancel")
            };
            var controller = await window.ShowProgressAsync(title, message, isCancelable, settings);
            return new ProgressDialogController(controller);
        }

        private static ImageSource MessageBoxImageToImageSource(MessageBoxImage image)
        {
            Icon icon;
            switch (image)
            {
                case MessageBoxImage.None:
                    icon = null;
                    break;
                case MessageBoxImage.Error: // also MessageBoxImage.Hand ans MessageBoxImage.Stop:
                    icon = SystemIcons.Hand;
                    break;
                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;
                case MessageBoxImage.Warning: // also MessageBoxImage.Exclamation
                    icon = SystemIcons.Exclamation;
                    break;
                case MessageBoxImage.Information: //case MessageBoxImage.Asterisk
                    icon = SystemIcons.Asterisk;
                    break;
                default:
                    icon = SystemIcons.Application;
                    break;
            }
            return (icon == null) ? null : Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        private static SystemSound MessageBoxImageToSystemSound(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.Hand: // also MessageBoxImage.Error ans MessageBoxImage.Stop:
                    return SystemSounds.Hand;
                case MessageBoxImage.Question:
                    return SystemSounds.Question;
                case MessageBoxImage.Exclamation: // also MessageBoxImage.Warning
                    return SystemSounds.Exclamation;
                case MessageBoxImage.Asterisk: //case MessageBoxImage.Information
                    return SystemSounds.Asterisk;
                default:
                    return SystemSounds.Beep;
            }
        }

        private static Task<MessageBoxResult> ShowAsync(this MetroWindow window, string message, string details = null,
            string title = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None)
        {
            var viewModel = new MetroMessageBoxViewModel(message, details, title, buttons,
                MessageBoxImageToImageSource(image));
            return ShowDialogAsync(window, viewModel, new MetroMessageBoxView(),
                    () => MessageBoxImageToSystemSound(image).Play());
        }
    }
}