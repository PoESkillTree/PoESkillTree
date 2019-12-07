using System.Collections.Generic;
using System.Threading.Tasks;
using PoESkillTree.Utils;
using PoESkillTree.Controls;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;

namespace PoESkillTree.Model.Serialization
{
    /// <summary>
    /// Abstract implementation of <see cref="IPersistentData"/> implementing all properties and the notify interfaces.
    /// </summary>
    public abstract class AbstractPersistentData : Notifier, IPersistentData
    {
#pragma warning disable CS8618 // _currentBuild and _equipmentData can't be null after initalization
        private Options _options = new Options();
        private PoEBuild _currentBuild;
        private IBuild? _selectedBuild;
        private EquipmentData _equipmentData;
#pragma warning restore

        public Options Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }

        public PoEBuild CurrentBuild
        {
            get => _currentBuild;
            set => SetProperty(ref _currentBuild, value);
        }

        public IBuild? SelectedBuild
        {
            get => _selectedBuild;
            set => SetProperty(ref _selectedBuild, value);
        }

        public List<StashBookmark> StashBookmarks { get; } = new List<StashBookmark>();

        public BuildFolder RootBuild { get; } = new BuildFolder {Name = "Root"};

        public List<Item> StashItems { get; } = new List<Item>();

        public IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; } =
            new Dictionary<string, IEnumerable<StashBookmark>>();

        public EquipmentData EquipmentData
        {
            get => _equipmentData;
            set => SetProperty(ref _equipmentData, value);
        }

        public abstract Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        public abstract void Save();

        public abstract void SaveFolders();

        public abstract void SaveBuild(IBuild build);

        public abstract void DeleteBuild(IBuild build);

        public abstract Task ReloadBuildsAsync();

        public abstract Task<PoEBuild?> ImportBuildAsync(string buildXml);

        public abstract string ExportBuild(PoEBuild build);
    }
}
