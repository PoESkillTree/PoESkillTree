using System.ComponentModel;
using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    public class SettingsMenuViewModel : CloseableViewModel
    {
        private readonly IDialogCoordinator _dialogCoordinator;

        public Options Options { get; }

        public SettingsMenuViewModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            Options = persistentData.Options;
            DisplayName = L10n.Message("Settings");

            PropertyChangedEventHandler handler = async (sender, args) => await OptionsChanged(args.PropertyName);
            Options.PropertyChanged += handler;
            RequestsClose += _ =>
            {
                Options.PropertyChanged -= handler;
                persistentData.SaveToFile();
            };
        }

        private async Task OptionsChanged(string propertyName)
        {
            if (propertyName == nameof(Options.Language) ||
                (propertyName == nameof(Options.DownloadMissingItemImages) && !Options.DownloadMissingItemImages))
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("You will need to restart the program for all changes to take effect."),
                    title: L10n.Message("Restart is needed"));

                if (propertyName == nameof(Options.Language))
                    L10n.Initialize(Options.Language);
            }
        }
    }
}