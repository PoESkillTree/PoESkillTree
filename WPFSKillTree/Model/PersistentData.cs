using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using POESKillTree.Controls;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class PersistentData : Notifier, IPersistentData
    {
        private Options _options = new Options();
        private PoEBuild _currentBuild;
        private PoEBuild _selectedBuild;
        private EquipmentData _equipmentData;

        public Options Options
        {
            get { return _options; }
            set { SetProperty(ref _options, value); }
        }

        public PoEBuild CurrentBuild
        {
            get { return _currentBuild; }
            set { SetProperty(ref _currentBuild, value); }
        }

        public PoEBuild SelectedBuild
        {
            get { return _selectedBuild; }
            set { SetProperty(ref _selectedBuild, value); }
        }

        public ObservableCollection<StashBookmark> StashBookmarks { get; } = new ObservableCollection<StashBookmark>();

        public BuildFolder RootBuild { get; } = new BuildFolder {Name = "Root"};

        public ObservableCollection<Item> StashItems { get; } = new ObservableCollection<Item>();

        public IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; } =
            new Dictionary<string, IEnumerable<StashBookmark>>();

        public EquipmentData EquipmentData
        {
            get { return _equipmentData; }
            set { SetProperty(ref _equipmentData, value); }
        }

        private readonly Func<IDialogCoordinator, Task> _initializer;

        public PersistentData()
        {
        }

        public PersistentData(Func<IDialogCoordinator, Task> initializer)
        {
            _initializer = initializer;
        }

        public event Action Initialized;

        public async Task InitializeAsync(IDialogCoordinator dialogCoordinator)
        {
            if (_initializer != null)
                await _initializer(dialogCoordinator);
            Initialized?.Invoke();
        }

        public event Action SaveHandler;

        public void Save()
        {
            SaveHandler?.Invoke();
        }

        public event Action<IBuild> SaveBuildHandler;

        public void SaveBuild(IBuild build)
        {
            SaveBuildHandler?.Invoke(build);
        }

        public event Action<IBuild> DeleteBuildHandler;

        public void DeleteBuild(IBuild build)
        {
            DeleteBuildHandler?.Invoke(build);
        }
    }
}
