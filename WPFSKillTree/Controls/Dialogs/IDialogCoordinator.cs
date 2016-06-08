using System.Threading.Tasks;
using System.Windows;

namespace POESKillTree.Controls.Dialogs
{
    public interface IDialogCoordinator
    {
        Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string title = null,
            MessageBoxImage image = MessageBoxImage.Question);

        Task ShowErrorAsync(object context, string message, string details = null, string title = null);

        Task ShowWarningAsync(object context, string message, string details = null, string title = null);

        Task ShowInfoAsync(object context, string message, string details = null, string title = null);

        Task<string> ShowInputAsync(object context, string title, string message);

        Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message,
            bool isCancelable = false);
    }
}