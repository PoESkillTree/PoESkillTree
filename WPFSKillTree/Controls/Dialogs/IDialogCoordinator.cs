using System;
using System.Threading.Tasks;
using System.Windows;

namespace POESKillTree.Controls.Dialogs
{
    public interface IDialogCoordinator
    {
        Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string details = null,
            string title = null, MessageBoxButton buttons = MessageBoxButton.YesNo,
            MessageBoxImage image = MessageBoxImage.Question);

        Task ShowErrorAsync(object context, string message, string details = null, string title = null);

        Task ShowWarningAsync(object context, string message, string details = null, string title = null);

        Task ShowInfoAsync(object context, string message, string details = null, string title = null);

        Task<string> ShowInputAsync(object context, string title, string message, string defaultText = "");

        Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message,
            bool isCancelable = false);

        /// <summary>
        /// Asynchronously shows a dialog where the user can select a file path.
        /// </summary>
        /// <param name="context">The context object that is registered by the view</param>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">A message shown in the dialog</param>
        /// <param name="defaultFile">The path that is initially selected</param>
        /// <param name="isFolderPicker">True, if the path is intepreted as a directory and not afile</param>
        /// <param name="isCancelable">True if the user can cancel the dialog</param>
        /// <returns>The selected path or <c>null</c> if the dialog is cancelable and was canceled</returns>
        Task<string> ShowFileSelectorAsync(object context, string title, string message, string defaultFile,
            bool isFolderPicker, bool isCancelable = true);

        Task<string> ShowValidatingInputDialogAsync(object context, string title, string message,
            string defaultText, Func<string, string> inputValidationFunc);
    }
}