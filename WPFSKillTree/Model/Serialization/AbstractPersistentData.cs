using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using POESKillTree.Controls;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Abstract implementation of <see cref="IPersistentData"/> implementing all properties and the notify interfaces.
    /// </summary>
    public abstract class AbstractPersistentData : Notifier, IPersistentData
    {
        private Options _options = new Options();
        private PoEBuild _currentBuild;
        private IBuild _selectedBuild;
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

        public IBuild SelectedBuild
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

        public abstract Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        public abstract void Save();

        public abstract void SaveFolders();

        public abstract void SaveBuild(IBuild build);

        public abstract void DeleteBuild(IBuild build);

        public abstract Task ReloadBuildsAsync();
    }
}
