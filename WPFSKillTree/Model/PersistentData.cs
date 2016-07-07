using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using POESKillTree.Controls;
using POESKillTree.Model.Items;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class PersistentData : Notifier, IPersistentData
    {
        // Only stuff that has a setter in IPersistentData needs to notify property changes.

        public Options Options { get; set; } = new Options();

        private PoEBuild _currentBuild;
        public PoEBuild CurrentBuild
        {
            get { return _currentBuild; }
            set { SetProperty(ref _currentBuild, value); }
        }

        private PoEBuild _selectedBuild;
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

        public EquipmentData EquipmentData { get; set; }

        public event EventHandler RequestsSave;

        public void SaveToFile()
        {
            RequestsSave?.Invoke(this, EventArgs.Empty);
        }
    }
}
