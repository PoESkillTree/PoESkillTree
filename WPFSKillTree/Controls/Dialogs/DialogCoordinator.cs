using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs.Views;
using PoESkillTree.Controls.Dialogs.ViewModels;

namespace PoESkillTree.Controls.Dialogs
{
    // Adjusted version of https://github.com/MahApps/MahApps.Metro/blob/1.3.0/MahApps.Metro/Controls/Dialogs/DialogCoordinator.cs 
    // (MIT licensed) that uses the methods of ExtendedDialogManager.
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
            var metroWindow = association.Invoke(() => Window.GetWindow(association) as MetroWindow);

            if (metroWindow == null)
                throw new InvalidOperationException("Control is not inside a MetroWindow.");
            return metroWindow;
        }

        protected static Task<T> ShowDialogAsync<T>(object context, CloseableViewModelBase<T> viewModel, BaseMetroDialog view,
            Action? onShown = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowDialogAsync(viewModel, view, onShown));
        }

        public Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string? details = null,
            string? title = null, MessageBoxButton buttons = MessageBoxButton.YesNo,
            MessageBoxImage image = MessageBoxImage.Question)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowQuestionAsync(message, details, title, buttons, image));
        }

        public Task ShowErrorAsync(object context, string message, string? details = null, string? title = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowErrorAsync(message, details, title));
        }

        public Task ShowWarningAsync(object context, string message, string? details = null, string? title = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowWarningAsync(message, details, title));
        }

        public Task ShowInfoAsync(object context, string message, string? details = null, string? title = null)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowInfoAsync(message, details, title));
        }

        public Task<string?> ShowInputAsync(object context, string title, string message, string defaultText = "")
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowInputAsync(title, message, defaultText));
        }

        public Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message,
            bool isCancelable = false)
        {
            var metroWindow = GetMetroWindow(context);
            return metroWindow.Invoke(() => metroWindow.ShowProgressAsync(title, message, isCancelable));
        }

        public Task<string?> ShowFileSelectorAsync(object context, string title, string message,
            FileSelectorDialogSettings settings)
        {
            return ShowDialogAsync(context,
                new FileSelectorViewModel(title, message, settings),
                new FileSelectorView());
        }

        public Task<string?> ShowValidatingInputDialogAsync(object context, string title, string message,
            string defaultText, Func<string, string?> inputValidationFunc)
        {
            return ShowDialogAsync(context,
                new ValidatingInputDialogViewModel(title, message, defaultText, inputValidationFunc),
                new ValidatingInputDialogView());
        }
    }
}