using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using POESKillTree.ViewModels;

namespace POESKillTree.Controls.Dialogs
{
    public interface IDialogCoordinator
    {
        Task ShowDialogAsync(object context, CloseableViewModel viewModel, BaseMetroDialog view, bool hideOnRequestsClos = true);

        Task HideDialogAsync(object context, BaseMetroDialog view);

        Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string title = null,
            MessageBoxImage image = MessageBoxImage.Question);

        Task ShowErrorAsync(object context, string message, string details = null, string title = null);

        Task ShowWarningAsync(object context, string message, string details = null, string title = null);

        Task ShowInfoAsync(object context, string message, string details = null, string title = null);
    }
}