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
    public abstract class AbstractPersistentData : Notifier, IPersistentData
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

        public abstract Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        public abstract void Save();

        public abstract void SaveBuild(IBuild build);

        public abstract void DeleteBuild(IBuild build);

        public abstract Task ReloadBuildsAsync();
    }

    /// <summary>
    /// Implements all abstract methods of <see cref="AbstractPersistentData"/> by throwing exceptions on call.
    /// Only use this class in tests or if the instance is immediately serialized and not used any further.
    /// </summary>
    public class BarePersistentData : AbstractPersistentData
    {
        public override Task InitializeAsync(IDialogCoordinator dialogCoordinator)
        {
            throw new System.NotSupportedException();
        }

        public override void Save()
        {
            throw new System.NotSupportedException();
        }

        public override void SaveBuild(IBuild build)
        {
            throw new System.NotSupportedException();
        }

        public override void DeleteBuild(IBuild build)
        {
            throw new System.NotSupportedException();
        }

        public override Task ReloadBuildsAsync()
        {
            throw new System.NotSupportedException();
        }
    }
}
