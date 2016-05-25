using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using POESKillTree.Utils.Extensions;
using POESKillTree.ViewModels;

namespace POESKillTree.Controls.Dialogs
{
    // Adjusted version of https://github.com/MahApps/MahApps.Metro/blob/develop/MahApps.Metro/Controls/Dialogs/DialogCoordinator.cs 
    // that uses the methods of ExtendedDialogManager and handles new context registrations if the context is
    // not registrated initially. With that, view models can show dialogs before their view is constructed.
    public class DialogCoordinator : IDialogCoordinator
    {
        /// <summary>
        /// Gets the default instance of the dialog coordinator, which can be injected into a view model.
        /// </summary>
        public static readonly DialogCoordinator Instance = new DialogCoordinator();

        private static async Task<MetroWindow> GetMetroWindowAsync(object context)
        {
            if (context == null) throw new ArgumentNullException("context");

            if (!DialogParticipation.IsRegistered(context))
            {
                var tcs = new TaskCompletionSource<DependencyObject>();
                ContextRegistrationChangedEventHandler handler = null;
                handler = (c, o) =>
                {
                    if (c == context)
                    {
                        DialogParticipation.ContextRegistrationChanged -= handler;
                        tcs.TrySetResult(o);
                    }
                };
                DialogParticipation.ContextRegistrationChanged += handler;

                var task = tcs.Task;
                if (await task.WithTimeout(TimeSpan.FromSeconds(2)))
                    return AssociationToWindow(task.Result);
                else
                    throw new InvalidOperationException(
                        "Context is not registered. Consider using DialogParticipation.Register in XAML to bind in the DataContext.");
            }

            var association = DialogParticipation.GetAssociation(context);
            return AssociationToWindow(association);
        }

        private static MetroWindow AssociationToWindow(DependencyObject association)
        {
            var metroWindow = Window.GetWindow(association) as MetroWindow;

            if (metroWindow == null)
                throw new InvalidOperationException("Control is not inside a MetroWindow.");
            return metroWindow;
        }

        protected async Task ShowDialogAsync(object context, CloseableViewModel viewModel, BaseMetroDialog view, Action onShown = null)
        {
            var metroWindow = await GetMetroWindowAsync(context);

            await metroWindow.ShowDialogAsync(viewModel, view, onShown);
        }

        public async Task<MessageBoxResult> ShowQuestionAsync(object context, string message, string title = null,
            MessageBoxImage image = MessageBoxImage.Question)
        {
            var metroWindow = await GetMetroWindowAsync(context);

            return await metroWindow.ShowQuestionAsync(message, title, image);
        }

        public async Task ShowErrorAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = await GetMetroWindowAsync(context);

            await metroWindow.ShowErrorAsync(message, details, title);
        }

        public async Task ShowWarningAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = await GetMetroWindowAsync(context);

            await metroWindow.ShowWarningAsync(message, details, title);
        }

        public async Task ShowInfoAsync(object context, string message, string details = null, string title = null)
        {
            var metroWindow = await GetMetroWindowAsync(context);

            await metroWindow.ShowInfoAsync(message, details, title);
        }
    }
}