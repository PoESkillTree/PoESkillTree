using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using POESKillTree.Controls;
using POESKillTree.Model.Items;

namespace POESKillTree.Model
{
    public interface IPersistentData : INotifyPropertyChanged, INotifyPropertyChanging
    {
        Options Options { get; }
        PoEBuild CurrentBuild { get; set; }
        PoEBuild SelectedBuild { get; set; }
        BuildFolder RootBuild { get; }
        EquipmentData EquipmentData { get; }
        ObservableCollection<Item> StashItems { get; }
        ObservableCollection<StashBookmark> StashBookmarks { get; }
        IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; }

        /// <summary>
        /// Triggers when <see cref="SaveToFile"/> is called.
        /// </summary>
        event EventHandler RequestsSave;
        void SaveToFile();
    }
}