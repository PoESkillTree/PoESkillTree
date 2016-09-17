using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs.Views;
using POESKillTree.Controls.Dialogs.ViewModels;

namespace POESKillTree.Controls.Dialogs
{
    // Adjusted version of https://github.com/MahApps/MahApps.Metro/blob/1.2.4/MahApps.Metro/Controls/Dialogs/DialogCoordinator.cs 
    // (licensed under Microsoft Public License as found on https://github.com/MahApps/MahApps.Metro/blob/1.2.4/LICENSE)
    // that uses the methods of ExtendedDialogManager.
    public class DialogCoordinator : IDialogCoordinator
    {
        /// <summary>
        /// Gets the default instance of the dialog coordinator, which can be injected into a view model.
        /// </summary>
        public static readonly IDialogCoordinator Instance = new DialogCoordinator();

        protected static MetroWindow GetMetroWindow(object context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!DialogParticipation.IsRegistered(context))
                throw new InvalidOperationException(
                    "Context is not registered. Consider using DialogParticipation.Register in XAML to bind in the DataContext.");

            var association = DialogParticipation.GetAssociation(context);
            var metroWindow = Window.GetWindow(association) as MetroWindow;

            if (metroWindow == null)
                throw new InvalidOperationException("Control is not inside a MetroWindow.");
            return metroWindow;
        }

        protected async Task<T> ShowDialogAsync<T>(object context, CloseableViewModel<T> viewModel, BaseMetroDialog view, Action onShown = null)
        {
            var metroWindow = GetMetroWindow(context);

            return await metroWindow.ShowDialogAsync(viewModel, view, onShown);
        }

        public async Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string details = null,
            string title = null, MessageBoxButton buttons = MessageBoxButton.YesNo,
            MessageBoxImage image = MessageBoxImage.Question)
        {
            var metroWindow = GetMetroWindow(context);

            return await metroWindow.ShowQuestionAsync(message, details, title, buttons, image);
        }

        public async Task ShowErrorAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = GetMetroWindow(context);

            await metroWindow.ShowErrorAsync(message, details, title);
        }

        public async Task ShowWarningAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = GetMetroWindow(context);

            await metroWindow.ShowWarningAsync(message, details, title);
        }

        public async Task ShowInfoAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = GetMetroWindow(context);

            await metroWindow.ShowInfoAsync(message, details, title);
        }

        public async Task<string> ShowInputAsync(object context, string title, string message, string defaultText = "")
        {
            var metroWindow = GetMetroWindow(context);
            return await metroWindow.ShowInputAsync(title, message, defaultText);
        }

        public async Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message,
            bool isCancelable = false)
        {
            var metroWindow = GetMetroWindow(context);
            return await metroWindow.ShowProgressAsync(title, message, isCancelable);
        }

        public Task<string> ShowFileSelectorAsync(object context, string title, string message,
            FileSelectorDialogSettings settings)
        {
            return ShowDialogAsync(context, new FileSelectorViewModel(title, message, settings), new FileSelectorView());
        }

        public async Task<string> ShowValidatingInputDialogAsync(object context, string title, string message,
            string defaultText, Func<string, string> inputValidationFunc)
        {
            return await ShowDialogAsync(context,
                new ValidatingInputDialogViewModel(title, message, defaultText, inputValidationFunc),
                new ValidatingInputDialogView());
        }
    }
}