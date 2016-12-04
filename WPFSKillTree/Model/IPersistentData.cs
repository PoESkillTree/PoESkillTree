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
    /// <summary>
    /// Enables access to all data that persists between program executions.
    /// </summary>
    public interface IPersistentData : INotifyPropertyChanged, INotifyPropertyChanging
    {
        Options Options { get; }
        PoEBuild CurrentBuild { get; set; }
        IBuild SelectedBuild { get; set; }
        BuildFolder RootBuild { get; }
        EquipmentData EquipmentData { get; }
        ObservableCollection<Item> StashItems { get; }
        ObservableCollection<StashBookmark> StashBookmarks { get; }
        IDictionary<string, IEnumerable<StashBookmark>> LeagueStashes { get; }

        /// <summary>
        /// Initializes all fields that require asnychronous actions like dialogs.
        /// </summary>
        Task InitializeAsync(IDialogCoordinator dialogCoordinator);

        /// <summary>
        /// Saves everything but the builds to the filesystem.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves all build folders to the filesystem.
        /// <para>
        /// This should only save the ordering of contained builds and IsExpanded. Every other changes to folders must
        /// be saved with <see cref="SaveBuild"/> directly after they done.
        /// </para>
        /// </summary>
        void SaveFolders();

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

        /// <summary>
        /// Asynchronously reloads all builds from the BuildsSavePath. All current builds are discarded.
        /// </summary>
        Task ReloadBuildsAsync();

        /// <summary>
        /// Asynchronously imports a PoEBuild from the given xml string and adds it to the root folder as current
        /// build if it could be imported successfully.
        /// </summary>
        Task<PoEBuild> ImportBuildAsync(string buildXml);

        /// <summary>
        /// Exports the PoEBuild as a xml string and returns that string.
        /// </summary>
        string ExportBuild(PoEBuild build);
    }
}