using System.Drawing;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.ViewModels;
using POESKillTree.Views;

namespace POESKillTree.Controls.Dialogs
{
    public static class DialogManager
    {
        /// <summary>
        /// If hideOnRequestsClose = false:
        ///     call and await this, you must manually hide it with "await window.HideDialogAsync(view)"
        /// <para/>
        /// else if you open the dialog in an event manager and do not do anything after that:
        ///     call (and optionally await) this
        /// <para/>
        /// else:
        ///     call and await this, then call and await "view.WaitUntilUnloadedAsync()"
        ///     (that waits until the view model's RequestsClose is raised and the dialog is closed)
        /// <para/>
        /// todo It is possible that "await view.WaitUntilUnloadedAsync()" won't work correctly if there are dialogs shown from the dialog
        ///      because that unloads the view.
        ///      If this is the case, hook into RequestsClose
        /// </summary>
        public static async Task ShowDialogAsync(this MetroWindow window, CloseableViewModel viewModel,
            BaseMetroDialog view, bool hideOnRequestsClose = true)
        {
            if (hideOnRequestsClose)
                viewModel.RequestsClose += () => window.HideMetroDialogAsync(view);
            view.DataContext = viewModel;
            view.Loaded += (sender, args) => DialogParticipation.SetRegister(view, viewModel);
            view.Unloaded += (sender, args) => DialogParticipation.SetRegister(view, null);

            await window.ShowMetroDialogAsync(view);
        }

        /// <summary>
        /// Only needs to be called if <see cref="ShowDialogAsync"/> is called with hideOnRequestsClose = false.
        /// </summary>
        public static async Task HideDialogAsync(this MetroWindow window, BaseMetroDialog view)
        {
            await window.HideMetroDialogAsync(view);
        }

        public static Task<MessageBoxResult> ShowQuestionAsync(this MetroWindow window, string message,
            string title = null, MessageBoxImage image = MessageBoxImage.Question)
        {
            return ShowAsync(window, message, title: title ?? L10n.Message("Confirmation"), buttons: MessageBoxButton.YesNo,
                image: image, defaultResult: MessageBoxResult.No);
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

        private static async Task<MessageBoxResult> ShowAsync(this MetroWindow window, string message, string details = null,
            string title = "", MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.OK)
        {
            var view = new MetroMessageBoxView();
            var viewModel = new MetroMessageBoxViewModel(message, details, title, buttons,
                MessageBoxImageToImageSource(image)) {Result = defaultResult};

            await ShowDialogAsync(window, viewModel, view);
            MessageBoxImageToSystemSound(image).Play();
            await view.WaitUntilUnloadedAsync();

            return viewModel.Result;
        }
    }
}