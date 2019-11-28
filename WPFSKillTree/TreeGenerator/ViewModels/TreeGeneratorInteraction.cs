using System;
using System.Threading.Tasks;
using System.Windows.Input;
using PoESkillTree.Utils;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.TreeGenerator.Views;

namespace PoESkillTree.TreeGenerator.ViewModels
{
    /// <summary>
    /// Class that encapsulates the interaction between MainWindow and the TreeGenerator namespace.
    /// </summary>
    public class TreeGeneratorInteraction : Notifier
    {
        private readonly IPersistentData _persistentData;
        private readonly ISettingsDialogCoordinator _dialogCoordinator;
        private SkillTree _skillTree;
        private SettingsViewModel _treeGeneratorViewModel;
        private SettingsWindow? _treeGeneratorWindow;

        public ICommand OpenTreeGeneratorCommand { get; }
        public ICommand RunTaggedNodesCommand { get; }
        public ICommand RunAdvancedCommand { get; }

        public SkillTree SkillTree
        {
            private get { return _skillTree; }
            set
            {
                SetProperty(ref _skillTree, value, () =>
                {
                    if (_treeGeneratorViewModel != null)
                        _treeGeneratorViewModel.RunFinished -= ViewModelRunFinished;
                    _treeGeneratorViewModel = new SettingsViewModel(SkillTree, SettingsDialogCoordinator.Instance, this);
                    _treeGeneratorViewModel.RunFinished += ViewModelRunFinished;
                    LoadSettings();
                });
            }
        }

#pragma warning disable CS8618 // _skillTree and _treeGeneratorViewModel are initialized set_SkillTree
        public TreeGeneratorInteraction(ISettingsDialogCoordinator dialogCoordinator, IPersistentData persistentData,
            SkillTree skillTree)
#pragma warning restore
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
            SkillTree = skillTree;

            OpenTreeGeneratorCommand = new AsyncRelayCommand(OpenTreeGenerator);
            RunTaggedNodesCommand = new AsyncRelayCommand(RunTaggedNodes);
            RunAdvancedCommand = new AsyncRelayCommand(RunAdvanced);
        }

        private void ViewModelRunFinished(object? sender, EventArgs args)
        {
            RunFinished?.Invoke(this, EventArgs.Empty);
        }

        private async Task OpenTreeGenerator()
        {
            if (_treeGeneratorWindow != null && _treeGeneratorWindow.IsOpen)
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("The Skill Tree Generator is already open"));
                return;
            }
            try
            {
                if (_treeGeneratorWindow == null)
                {
                    _treeGeneratorWindow = new SettingsWindow { DataContext = _treeGeneratorViewModel };
                    _treeGeneratorWindow.Closing += (o, args) => SaveSettings();
                }
                await _dialogCoordinator.ShowChildWindowAsync(this, _treeGeneratorWindow);
            }
            catch (Exception ex)
            {
                await _dialogCoordinator.ShowErrorAsync(this,
                    L10n.Message("Could not open Skill Tree Generator"), ex.Message);
#if DEBUG
                throw;
#endif
            }
        }

        private async Task RunGenerator(int index)
        {
            var generator = _treeGeneratorViewModel.Tabs[index];
            await _treeGeneratorViewModel.RunAsync(generator);
        }

        private async Task RunTaggedNodes()
        {
            await RunGenerator(0);
        }

        private async Task RunAdvanced()
        {
            await RunGenerator(1);
        }

        public void SaveSettings()
        {
            if (_treeGeneratorViewModel.SaveTo(_persistentData.CurrentBuild.AdditionalData))
            {
                _persistentData.CurrentBuild.FlagDirty();
            }
        }

        public void LoadSettings()
        {
            _treeGeneratorViewModel.LoadFrom(_persistentData.CurrentBuild.AdditionalData);
        }

        public event EventHandler? RunFinished;
    }
}