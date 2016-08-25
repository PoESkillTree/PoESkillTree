using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Serialization;

namespace POESKillTree.ViewModels
{
    public class SettingsMenuViewModel : CloseableViewModel
    {
        private readonly IPersistentData _persistentData;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly BuildsControlViewModel _buildsControlViewModel;

        public Options Options { get; }

        public ICommand ChangeBuildsSavePathCommand { get; }

        public SettingsMenuViewModel(IPersistentData persistentData, IDialogCoordinator dialogCoordinator,
            BuildsControlViewModel buildsControlViewModel)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            _buildsControlViewModel = buildsControlViewModel;
            Options = persistentData.Options;
            DisplayName = L10n.Message("Settings");
            ChangeBuildsSavePathCommand = new AsyncRelayCommand(ChangeBuildsSavePath);

            PropertyChangedEventHandler handler = async (sender, args) => await OptionsChanged(args.PropertyName);
            Options.PropertyChanged += handler;
            RequestsClose += _ =>
            {
                Options.PropertyChanged -= handler;
                persistentData.Save();
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

        private async Task ChangeBuildsSavePath()
        {
            var message = L10n.Message("There are unsaved builds. Do you want to save them before changing build directory?\n\n"
                                       + "If you cancel, the build directory will not be changed.");
            if (!await _buildsControlViewModel.HandleUnsavedBuilds(message))
                return;
            var dialogSettings = new FileSelectorDialogSettings
            {
                DefaultPath = Options.BuildsSavePath,
                IsFolderPicker = true,
                ValidationSubPath = SerializationConstants.EncodedDefaultBuildName
            };
            var path = await _dialogCoordinator.ShowFileSelectorAsync(this,
                L10n.Message("Select build directory"),
                L10n.Message("Select the directory where builds will be stored.\n" +
                             "It will be created if it does not yet exist."),
                dialogSettings);
            if (path == null)
                return;
            Options.BuildsSavePath = path;
            await _persistentData.ReloadBuildsAsync();
        }
    }
}