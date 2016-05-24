using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;

namespace POESKillTree.ViewModels
{
    public class SettingsMenuViewModel : CloseableViewModel
    {
        public static IEnumerable<string> Languages
        {
            get { return L10n.GetLanguages().Keys; }
        }

        private readonly IDialogCoordinator _dialogCoordinator;

        public Options Options { get; private set; }

        public SettingsMenuViewModel(PersistentData persistentData, IDialogCoordinator dialogCoordinator)
        {
            _dialogCoordinator = dialogCoordinator;
            Options = persistentData.Options;
            DisplayName = L10n.Message("Settings");

            PropertyChangedEventHandler handler = async (sender, args) => await OptionsChanged(args.PropertyName);
            Options.PropertyChanged += handler;
            RequestsClose += () =>
            {
                Options.PropertyChanged -= handler;
                persistentData.SavePersistentDataToFile();
            };
        }

        private async Task OptionsChanged(string propertyName)
        {
            if (propertyName == "Language")
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("You will need to restart the program for all changes to take effect."),
                    title: L10n.Message("Restart is needed"));

                L10n.SetLanguage(Options.Language);
            }
        }
    }
}