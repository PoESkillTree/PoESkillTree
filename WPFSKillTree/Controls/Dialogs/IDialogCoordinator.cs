using System;
using System.Threading.Tasks;
using System.Windows;

namespace PoESkillTree.Controls.Dialogs
{
    public interface IDialogCoordinator
    {
        Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string? details = null,
            string? title = null, MessageBoxButton buttons = MessageBoxButton.YesNo,
            MessageBoxImage image = MessageBoxImage.Question);

        Task ShowErrorAsync(object context, string message, string? details = null, string? title = null);

        Task ShowWarningAsync(object context, string message, string? details = null, string? title = null);

        Task ShowInfoAsync(object context, string message, string? details = null, string? title = null);

        /// <summary>
        /// Asynchronously shows a dialog where the user can input text.
        /// </summary>
        /// <param name="context">The context object that is registered by the view</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">A message shown in the dialog</param>
        /// <param name="defaultText">The text that the input box initially contains</param>
        /// <returns>The text entered by the user or null if the dialog was canceled</returns>
        Task<string?> ShowInputAsync(object context, string title, string message, string defaultText = "");

        Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message,
            bool isCancelable = false);

        /// <summary>
        /// Asynchronously shows a dialog where the user can select a file path.
        /// </summary>
        /// <param name="context">The context object that is registered by the view</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">A message shown in the dialog</param>
        /// <param name="settings">An instance that further controls what the dialog does</param>
        /// <returns>The selected path or <c>null</c> if the dialog is cancelable and was canceled</returns>
        Task<string?> ShowFileSelectorAsync(object context, string title, string message,
            FileSelectorDialogSettings settings);

        /// <summary>
        /// Asynchronously shows a dialog where the user can input text that is validated.
        /// </summary>
        /// <param name="context">The context object that is registered by the view</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">A message shown in the dialog</param>
        /// <param name="defaultText">The text that the input box initially contains</param>
        /// <param name="inputValidationFunc">A function that returns an error message for a entered string.
        /// Returns null or an empty string if there are no errors with the input.</param>
        /// <returns>The text entered by the user or null if the dialog was canceled</returns>
        Task<string?> ShowValidatingInputDialogAsync(object context, string title, string message,
            string defaultText, Func<string, string?> inputValidationFunc);
    }
}