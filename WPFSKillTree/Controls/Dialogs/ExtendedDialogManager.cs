using System;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.IconPacks;
using PoESkillTree.Utils.Extensions;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs.ViewModels;
using PoESkillTree.Controls.Dialogs.Views;
using PoESkillTree.Localization;

namespace PoESkillTree.Controls.Dialogs
{
    public static class ExtendedDialogManager
    {
        /// <summary>
        /// Sets up the connection between view and viewModel, shows the view as a metro dialog,
        /// calls <paramref name="onShown"/> and waits for the dialog to be closed.
        /// </summary>
        public static async Task<T> ShowDialogAsync<T>(this MetroWindow window, CloseableViewModelBase<T> viewModel,
            BaseMetroDialog view, Action onShown = null)
        {
            view.DataContext = viewModel;

            // Undefault buttons not in the dialog as they would be pressed instead of a dialog button on enter.
            var oldDefaults = window.FindVisualChildren<Button>().Where(b => b.IsDefault).ToList();
            oldDefaults.ForEach(b => b.IsDefault = false);
            // Save old keyboard focus.
            var oldFocus = Keyboard.FocusedElement;

            await window.ShowMetroDialogAsync(view, new MetroDialogSettings {AnimateShow = false});
            // Focus the first focusable element in the view
            var element = view.FindVisualChildren<UIElement>().FirstOrDefault(e => e.Focusable);
            element?.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Keyboard.Focus(element)));
            onShown?.Invoke();

            var result = await viewModel.WaitForCloseAsync();
            await window.HideMetroDialogAsync(view, new MetroDialogSettings {AnimateHide = false});

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

        private static PackIconModernKind? MessageBoxImageToImageKind(MessageBoxImage image)
        {
            switch (image)
            {
                case MessageBoxImage.None:
                    return null;
                case MessageBoxImage.Error: // also MessageBoxImage.Hand ans MessageBoxImage.Stop:
                    return PackIconModernKind.Stop;
                case MessageBoxImage.Question:
                    return null;
                case MessageBoxImage.Warning: // also MessageBoxImage.Exclamation
                    return PackIconModernKind.Warning;
                case MessageBoxImage.Information: //case MessageBoxImage.Asterisk
                    return PackIconModernKind.InformationCircle;
                default:
                    return null;
            }
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
                MessageBoxImageToImageKind(image));
            return ShowDialogAsync(window, viewModel, new MetroMessageBoxView(),
                    () => MessageBoxImageToSystemSound(image).Play());
        }
    }
}