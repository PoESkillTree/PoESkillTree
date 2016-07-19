using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using POESKillTree.Controls;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;
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

        Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        /// <summary>
        /// Saves everything but the builds to the filesystem.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the given build to the filesystem.
        /// </summary>
        void SaveBuild(IBuild build);

        /// <summary>
        /// Deletes the given build from the filesystem.
        /// This has to be called after its folder has been saved as it can't be known if it was moved or
        /// deleted when saving the folder.
        /// </summary>
        void DeleteBuild(IBuild build);
    }
}