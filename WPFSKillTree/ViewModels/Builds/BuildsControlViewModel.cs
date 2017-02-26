using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using log4net;
using MoreLinq;
using POESKillTree.Common;
using POESKillTree.Common.ViewModels;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels.Builds
{
    public class ClassFilterItem
    {
        public string CharacterClass { get; }

        public string AscendancyClass { get; }

        public ClassFilterItem(string characterClass, string ascendancyClass)
        {
            CharacterClass = characterClass;
            AscendancyClass = ascendancyClass;
        }

        public override string ToString()
        {
            return CharacterClass + " " + AscendancyClass;
        }
    }

    public class BuildsViewModelProxy : BindingProxy<BuildsControlViewModel>
    {
    }

    /// <summary>
    /// View model providing access to and operations on builds.
    /// </summary>
    public class BuildsControlViewModel : Notifier
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildsControlViewModel));

        private static readonly ClassFilterItem NoFilterItem = new ClassFilterItem(L10n.Message("All"), null);

        private readonly IExtendedDialogCoordinator _dialogCoordinator;

        private readonly BuildValidator _buildValidator;

        private readonly FileSystemWatcher _fileSystemWatcher;
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;
        private readonly SimpleMonitor _changingFileSystemMonitor = new SimpleMonitor();
        private int _changingFileSystemCounter;

        /// <summary>
        /// Gets the root folder of the build folder tree.
        /// </summary>
        public IBuildFolderViewModel BuildRoot { get; }

        /// <summary>
        /// Gets the drop handler that handles drag and drop for the build tree.
        /// </summary>
        public IDropTarget DropHandler { get; }

        public ICommand NewFolderCommand { get; }

        public ICommand NewBuildCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand OpenBuildCommand { get; }

        public ICommand SaveBuildCommand { get; }

        public ICommand SaveBuildAsCommand { get; }

        public ICommand SaveAllBuildsCommand { get; }

        public ICommand RevertBuildCommand { get; }

        public ICommand MoveUpCommand { get; }

        public ICommand MoveDownCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand CutCommand { get; }

        public ICommand CopyCommand { get; }

        public ICommand PasteCommand { get; }

        public ICommand ReloadCommand { get; }

        public ICommand OpenBuildsSavePathCommand { get; }

        public ICommand ExpandAllCommand { get; }

        public ICommand CollapseAllCommand { get; }

        public ICommand ExportCurrentToClipboardCommand { get; }
        public ICommand ImportCurrentFromClipboardCommand { get; }

        public IPersistentData PersistentData { get; }

        private BuildViewModel _currentBuild;
        /// <summary>
        /// Gets or sets the currently opened build.
        /// </summary>
        public BuildViewModel CurrentBuild
        {
            get { return _currentBuild; }
            set
            {
                SetProperty(ref _currentBuild, value, () =>
                {
                    if (CurrentBuild != null)
                        CurrentBuild.CurrentlyOpen = true;
                    PersistentData.CurrentBuild = CurrentBuild?.Build;
                }, b =>
                {
                    if (CurrentBuild != null)
                        CurrentBuild.CurrentlyOpen = false;
                });
            }
        }

        private IBuildViewModel _selectedBuild;
        /// <summary>
        /// Gets or sets the currently selected build.
        /// </summary>
        public IBuildViewModel SelectedBuild
        {
            get { return _selectedBuild; }
            set
            {
                SetProperty(ref _selectedBuild, value, () =>
                {
                    if (SelectedBuild != null)
                        SelectedBuild.IsSelected = true;
                    PersistentData.SelectedBuild = SelectedBuild?.Build;
                }, b =>
                {
                    if (SelectedBuild != null)
                        SelectedBuild.IsSelected = false;
                });
            }
        }

        private ClassFilterItem _classFilter;
        /// <summary>
        /// Gets or sets the class builds must be of to be visible
        /// (L10n("All") shows everything).
        /// </summary>
        public ClassFilterItem ClassFilter
        {
            get { return _classFilter; }
            set { SetProperty(ref _classFilter, value, () => BuildRoot.ApplyFilter()); }
        }

        private string _textFilter;
        /// <summary>
        /// Gets or sets a string build names must contain to be visible.
        /// </summary>
        public string TextFilter
        {
            get { return _textFilter; }
            set { SetProperty(ref _textFilter, value, () => BuildRoot.ApplyFilter()); }
        }

        private IBuildViewModel _buildClipboard;
        private bool _clipboardIsCopy;

        private ISkillTree _skillTree;
        /// <summary>
        /// Sets the <see cref="ISkillTree"/> instance.
        /// </summary>
        public ISkillTree SkillTree
        {
            private get { return _skillTree; }
            set { SetProperty(ref _skillTree, value, () => BuildRoot.SkillTree = SkillTree); }
        }

        public IReadOnlyList<ClassFilterItem> ClassFilterItems { get; }

        public BuildsControlViewModel(IExtendedDialogCoordinator dialogCoordinator, IPersistentData persistentData, ISkillTree skillTree)
        {
            _dialogCoordinator = dialogCoordinator;
            PersistentData = persistentData;
            DropHandler = new CustomDropHandler(this);
            _buildValidator = new BuildValidator(PersistentData.Options);
            BuildRoot = new BuildFolderViewModel(persistentData.RootBuild, Filter, BuildOnCollectionChanged);

            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = PersistentData.Options.BuildsSavePath,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite
            };
            _fileSystemWatcher.Error += FileSystemWatcherOnError;
            _fileSystemWatcher.Changed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Created += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Deleted += FileSystemWatcherOnChanged;
            _fileSystemWatcher.Renamed += FileSystemWatcherOnChanged;
            _fileSystemWatcher.EnableRaisingEvents = true;

            // The monitor alone is not enough because delays are necessary and those shouldn't block other save
            // operations, which would happen if delays are awaited directly in the save method.
            // It could be done awaited with .ConfigureAwait(false) if SimpleMonitor would be thread safe.
            _changingFileSystemMonitor.Entered += (sender, args) => _changingFileSystemCounter++;
            _changingFileSystemMonitor.Freed += async (sender, args) =>
            {
                // Wait because FileSystemWatcherOnChanged calls are delayed a bit.
                await Task.Delay(2000);
                // This is a counter and not boolean because other save operations may happen while waiting on delay.
                _changingFileSystemCounter--;
            };

            CurrentBuild = TreeFindBuildViewModel(PersistentData.CurrentBuild);
            SelectedBuild = TreeFindBuildViewModel(PersistentData.SelectedBuild);
            PersistentData.PropertyChanged += PersistentDataOnPropertyChanged;
            PersistentData.Options.PropertyChanged += OptionsOnPropertyChanged;

            NewFolderCommand = new AsyncRelayCommand<IBuildFolderViewModel>(
                NewFolder,
                vm => vm != null && _buildValidator.CanHaveSubfolder(vm));
            NewBuildCommand = new RelayCommand<IBuildFolderViewModel>(NewBuild);
            DeleteCommand = new AsyncRelayCommand<IBuildViewModel>(
                Delete,
                o => o != BuildRoot);
            OpenBuildCommand = new AsyncRelayCommand<BuildViewModel>(
                OpenBuild,
                b => b != null && (b != CurrentBuild || b.Build.IsDirty));
            SaveBuildCommand = new AsyncRelayCommand<BuildViewModel>(
                SaveBuild,
                b => b != null && b.Build.IsDirty);
            SaveBuildAsCommand = new AsyncRelayCommand<BuildViewModel>(SaveBuildAs);
            SaveAllBuildsCommand = new AsyncRelayCommand(
                SaveAllBuilds,
                () => TreeFind<BuildViewModel>(b => b.Build.IsDirty, BuildRoot) != null);
            RevertBuildCommand = new RelayCommand<BuildViewModel>(
                build => build.Build.RevertChanges(),
                b => b != null && b.Build.IsDirty && b.Build.CanRevert);
            MoveUpCommand = new RelayCommand<IBuildViewModel>(
                MoveUp,
                o => o != BuildRoot && o.Parent.Children.IndexOf(o) > 0);
            MoveDownCommand = new RelayCommand<IBuildViewModel>(
                MoveDown,
                o => o != BuildRoot && o.Parent.Children.IndexOf(o) < o.Parent.Children.Count - 1);
            EditCommand = new AsyncRelayCommand<IBuildViewModel>(Edit);
            CutCommand = new AsyncRelayCommand<IBuildViewModel>(
                Cut,
                b => b != BuildRoot && b != CurrentBuild);
            CopyCommand = new RelayCommand<IBuildViewModel<PoEBuild>>(Copy);
            PasteCommand = new AsyncRelayCommand<IBuildViewModel>(Paste, CanPaste);
            ReloadCommand = new AsyncRelayCommand(Reload);
            OpenBuildsSavePathCommand = new RelayCommand(() => Process.Start(PersistentData.Options.BuildsSavePath));
            ExpandAllCommand = new RelayCommand(ExpandAll);
            CollapseAllCommand = new RelayCommand(CollapseAll);
            ExportCurrentToClipboardCommand = new RelayCommand(() => CopyToClipboard(CurrentBuild.Build));
            ImportCurrentFromClipboardCommand = new AsyncRelayCommand(ImportCurrentFromClipboard, CanPasteFromClipboard);

            SkillTree = skillTree;
            ClassFilterItems = GenerateAscendancyClassItems(SkillTree.AscendancyClasses).ToList();
            ClassFilter = NoFilterItem;
        }

        private IEnumerable<ClassFilterItem> GenerateAscendancyClassItems(IAscendancyClasses ascendancyClasses)
        {
            yield return NoFilterItem;
            foreach (var nameToContent in CharacterNames.NameToContent)
            {
                var charClass = nameToContent.Value;
                yield return new ClassFilterItem(charClass, null);

                foreach (var ascClass in ascendancyClasses.AscendancyClassesForCharacter(charClass))
                {
                    yield return new ClassFilterItem(charClass, ascClass);
                }
            }
        }

        #region Event handlers

        private void PersistentDataOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IPersistentData.CurrentBuild):
                    if (CurrentBuild?.Build == PersistentData.CurrentBuild)
                        return;
                    CurrentBuild = PersistentData.CurrentBuild == null
                        ? null
                        : TreeFindBuildViewModel(PersistentData.CurrentBuild);
                    break;
                case nameof(IPersistentData.SelectedBuild):
                    if (SelectedBuild?.Build == PersistentData.SelectedBuild)
                        return;
                    SelectedBuild = PersistentData.SelectedBuild == null
                        ? null
                        : TreeFindBuildViewModel(PersistentData.SelectedBuild);
                    break;
            }
        }

        private void OptionsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Options.BuildsSavePath):
                    Directory.CreateDirectory(PersistentData.Options.BuildsSavePath);
                    _fileSystemWatcher.Path = PersistentData.Options.BuildsSavePath;
                    break;
            }
        }

        private void FileSystemWatcherOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            Log.Error($"File system watcher for {_fileSystemWatcher.Path} stopped working",
                errorEventArgs.GetException());
        }

        private void FileSystemWatcherOnChanged(object sender, EventArgs fileSystemEventArgs)
        {
            // Only continue if all operations done by ourselves and the delay periods after them are finished.
            if (_changingFileSystemCounter > 0)
                return;
            _synchronizationContext.Post(async _ => await FileSystemWatcherOnChanged(), null);
        }

        private async Task FileSystemWatcherOnChanged()
        {
            // There might be multiple changes, only react to the first one.
            // Events for changes that take some time should occur before they are reenabled.
            _fileSystemWatcher.EnableRaisingEvents = false;
            var message = L10n.Message("Files in your build save directory have been changed.\n" +
                                       "Do you want to reload all builds from the file system?");
            if (GetDirtyBuilds().Any())
            {
                message += L10n.Message("\nAll unsaved changes will be lost.");
            }
            message += L10n.Message("\n\nYou can also reload through the 'File' menu.");
            var result =
                await _dialogCoordinator.ShowQuestionAsync(this, message, title: L10n.Message("Builds changed"));
            if (result == MessageBoxResult.Yes)
            {
                await PersistentData.ReloadBuildsAsync();
            }
            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        #endregion

        #region Command methods

        private async Task NewFolder(IBuildFolderViewModel folder)
        {
            var name = await _dialogCoordinator.ShowValidatingInputDialogAsync(this,
                L10n.Message("New Folder"),
                L10n.Message("Enter the name of the new folder."),
                "",
                s => _buildValidator.ValidateNewFolderName(s, folder));
            if (string.IsNullOrWhiteSpace(name))
                return;
            var newFolder = new BuildFolderViewModel(new BuildFolder {Name = name}, Filter, BuildOnCollectionChanged);
            folder.Children.Add(newFolder);
            await SaveBuildToFile(newFolder);
        }

        public void NewBuild(IBuildFolderViewModel folder)
        {
            var name = Util.FindDistinctName(SerializationConstants.DefaultBuildName,
                folder.Children.Select(b => b.Build.Name));
            var build = new BuildViewModel(new PoEBuild { Name = name }, Filter);
            folder.Children.Add(build);
            CurrentBuild = build;
        }

        private async Task Delete(IBuildViewModel build)
        {
            if (TreeFind<BuildViewModel>(b => b == CurrentBuild, build) != null)
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("The currently opened build can not be deleted."));
                return;
            }
            if (build is IBuildFolderViewModel)
            {
                var result = await _dialogCoordinator.ShowQuestionAsync(this,
                    string.Format(L10n.Message("This will delete the build folder \"{0}\" and all its contents.\n"),
                        build.Build.Name) + L10n.Message("Do you want to continue?"));
                if (result != MessageBoxResult.Yes)
                    return;
            }
            build.IsSelected = false;
            build.Parent.IsSelected = true;
            build.Parent.Children.Remove(build);
            await DeleteBuildFile(build);
        }

        private async Task OpenBuild(BuildViewModel build)
        {
            if (build != CurrentBuild)
            {
                CurrentBuild = build;
                return;
            }
            if (!build.Build.IsDirty || !build.Build.CanRevert)
                return;

            var result = await _dialogCoordinator.ShowQuestionAsync(this,
                L10n.Message("This build is currently opened but has unsaved changes.\n")
                + L10n.Message("Do you want to discard all changes made to it since your last save?"),
                title: L10n.Message("Discard changes?"));
            if (result == MessageBoxResult.Yes)
            {
                CurrentBuild.Build.RevertChanges();
            }
        }

        private async Task SaveBuild(BuildViewModel build)
        {
            var poeBuild = build.Build;
            if (!poeBuild.CanRevert && poeBuild.IsDirty)
            {
                // Build was created in this program run and was not yet saved.
                // Thus it needs a name.
                var name = await _dialogCoordinator.ShowValidatingInputDialogAsync(this,
                    L10n.Message("Saving new Build"),
                    L10n.Message("Enter the name of the build."),
                    poeBuild.Name,
                    s => _buildValidator.ValidateExistingFileName(s, build));
                if (string.IsNullOrEmpty(name))
                    return;
                poeBuild.Name = name;
            }
            poeBuild.LastUpdated = DateTime.Now;
            await SaveBuildToFile(build);
            // Save parent folder to retain ordering information when renaming
            await SaveBuildToFile(build.Parent);
        }

        private async Task SaveBuildAs(BuildViewModel vm)
        {
            var build = vm.Build;
            var name = await _dialogCoordinator.ShowValidatingInputDialogAsync(this, L10n.Message("Save as"),
                L10n.Message("Enter the new name of the build"), build.Name, s => _buildValidator.ValidateNewBuildName(s, vm.Parent));
            if (string.IsNullOrWhiteSpace(name))
                return;
            var newBuild = build.DeepClone();
            newBuild.Name = name;
            var newVm = new BuildViewModel(newBuild, Filter);

            var builds = vm.Parent.Children;
            if (build.CanRevert)
            {
                // The original build exists in the file system.
                build.RevertChanges();
                builds.Insert(builds.IndexOf(vm), newVm);
            }
            else
            {
                // The original build does not exist in the file system
                // It will be replaced by the new one.
                var i = builds.IndexOf(vm);
                builds.RemoveAt(i);
                builds.Insert(i, newVm);
            }

            CurrentBuild = newVm;
            await SaveBuild(newVm);
        }

        private async Task SaveAllBuilds()
        {
            await TreeTraverseAsync<BuildViewModel>(async build =>
            {
                if (build.Build.IsDirty)
                    await SaveBuild(build);
            }, BuildRoot);
        }

        private void MoveUp(IBuildViewModel build)
        {
            var list = build.Parent.Children;
            var i = list.IndexOf(build);
            list.Move(i, i - 1);
        }

        private void MoveDown(IBuildViewModel build)
        {
            var list = build.Parent.Children;
            var i = list.IndexOf(build);
            list.Move(i, i + 1);
        }

        private async Task Cut(IBuildViewModel build)
        {
            build.IsSelected = false;
            build.Parent.IsSelected = true;
            build.Parent.Children.Remove(build);
            await DeleteBuildFile(build);
            _buildClipboard = build;
            _clipboardIsCopy = false;

            var poeBuild = build.Build as PoEBuild;
            if (poeBuild != null)
            {
                CopyToClipboard(poeBuild);
            }
        }

        private void Copy(IBuildViewModel<PoEBuild> build)
        {
            _buildClipboard = build;
            _clipboardIsCopy = true;
            CopyToClipboard(build.Build);
        }

        private bool CanPaste(IBuildViewModel target)
        {
            return CanPasteFromClipboard() || CanPasteNonClipboard(target);
        }

        private bool CanPasteNonClipboard(IBuildViewModel target)
        {
            if (target == null || _buildClipboard == null)
                return false;
            var targetFolder = target as IBuildFolderViewModel ?? target.Parent;
            return _buildValidator.CanMoveTo(_buildClipboard, targetFolder);
        }

        private async Task Paste(IBuildViewModel target)
        {
            var targetFolder = target as IBuildFolderViewModel ?? target.Parent;
            IBuildViewModel pasted;

            if (CanPasteFromClipboard() && !CanPasteNonClipboard(target))
            {
                var b = await PasteFromClipboard(BuildRoot);
                if (b == null)
                    return;
                pasted = new BuildViewModel(b, Filter);
            }
            else if (_clipboardIsCopy)
            {
                var newBuild = _buildClipboard.Build.DeepClone() as PoEBuild;
                if (newBuild == null)
                    throw new InvalidOperationException("Can only copy builds, not folders.");
                newBuild.Name = Util.FindDistinctName(newBuild.Name, targetFolder.Children.Select(b => b.Build.Name));
                pasted = new BuildViewModel(newBuild, Filter);
            }
            else
            {
                pasted = _buildClipboard;
                _buildClipboard = null;
            }
            targetFolder.Children.Add(pasted);

            // Folders and non-dirty builds need to be saved to create the new file.
            var build = pasted.Build as PoEBuild;
            if (build == null || !build.IsDirty)
            {
                await SaveBuildToFile(pasted);
            }
        }

        private async Task Edit(IBuildViewModel build)
        {
            var nameBefore = build.Build.Name;
            var buildVm = build as IBuildViewModel<PoEBuild>;
            var folderVm = build as IBuildViewModel<BuildFolder>;
            if (buildVm != null)
            {
                await _dialogCoordinator.EditBuildAsync(this, buildVm, _buildValidator);
            }
            else if (folderVm != null)
            {
                var name = await _dialogCoordinator.ShowValidatingInputDialogAsync(this,
                    L10n.Message("Edit Folder"),
                    L10n.Message("Enter the new name for this folder below."),
                    folderVm.Build.Name,
                    s => _buildValidator.ValidateExistingFolderName(s, folderVm));
                if (!string.IsNullOrWhiteSpace(name))
                {
                    folderVm.Build.Name = name;
                    await SaveBuildToFile(build);
                }
            }
            else
            {
                throw new ArgumentException("Argument's IBuild implementation is not supported");
            }
            if (build.Build.Name != nameBefore)
            {
                await SaveBuildToFile(build.Parent);
            }
        }

        private async Task Reload()
        {
            if (GetDirtyBuilds().Any())
            {
                var result = await _dialogCoordinator.ShowQuestionAsync(this,
                    L10n.Message("Any unsaved changes will be lost.\nAre you sure?"));
                if (result != MessageBoxResult.Yes)
                    return;
            }
            await PersistentData.ReloadBuildsAsync();
        }

        private void ExpandAll()
        {
            TreeTraverse<IBuildFolderViewModel>(b => b.Build.IsExpanded = true, BuildRoot);
        }

        private void CollapseAll()
        {
            TreeTraverse<IBuildFolderViewModel>(b => b.Build.IsExpanded = false, BuildRoot);
        }

        private void CopyToClipboard(PoEBuild build)
        {
            Clipboard.SetText(PersistentData.ExportBuild(build));
        }

        private static bool CanPasteFromClipboard()
        {
            if (!Clipboard.ContainsText())
            {
                return false;
            }
            var str = Clipboard.GetText();
            return str.StartsWith("<?xml ") && str.Contains("<PoEBuild ") && str.Contains("</PoEBuild>");
        }

        private async Task<PoEBuild> PasteFromClipboard(IBuildFolderViewModel targetFolder)
        {
            var str = Clipboard.GetText();
            var newBuild = await PersistentData.ImportBuildAsync(str);
            if (newBuild == null)
                return null;
            newBuild.Name = Util.FindDistinctName(newBuild.Name, targetFolder.Children.Select(b => b.Build.Name));
            return newBuild;
        }

        private async Task ImportCurrentFromClipboard()
        {
            var pasted = await PasteFromClipboard(BuildRoot);
            if (pasted == null)
                return;
            var build = new BuildViewModel(pasted, Filter);
            BuildRoot.Children.Add(build);
            CurrentBuild = build;
        }

        #endregion

        private bool Filter(IBuildViewModel b)
        {
            if (ClassFilter == null)
                return true;
            var build = b as BuildViewModel;
            if (build == null)
                return true;
            if (ClassFilter != NoFilterItem)
            {
                if (ClassFilter.CharacterClass != build.CharacterClass)
                    return false;
                if (ClassFilter.AscendancyClass != null
                    && ClassFilter.AscendancyClass != build.AscendancyClass)
                {
                    return false;
                }
            }
            return string.IsNullOrEmpty(TextFilter)
                || build.Build.Name.Contains(TextFilter, StringComparison.InvariantCultureIgnoreCase);
        }

        #region Traverse helper methods

        private BuildViewModel TreeFindBuildViewModel(PoEBuild build)
        {
            return TreeFind<BuildViewModel>(b => b.Build == build, BuildRoot);
        }

        private IBuildViewModel<T> TreeFindBuildViewModel<T>(T build)
            where T : class, IBuild
        {
            return TreeFind<IBuildViewModel<T>>(b => b.Build == build, BuildRoot);
        }

        private static T TreeFind<T>(Predicate<T> predicate, IBuildViewModel current) where T : class, IBuildViewModel
        {
            var t = current as T;
            if (t != null && predicate(t))
            {
                return t;
            }
            var folder = current as BuildFolderViewModel;
            return folder?.Children.Select(build => TreeFind(predicate, build)).FirstOrDefault(r => r != null);
        }

        private static async Task TreeTraverseAsync<T>(Func<T, Task> action, IBuildViewModel current) where T : class, IBuildViewModel
        {
            var t = current as T;
            if (t != null)
                await action(t);
            var folder = current as BuildFolderViewModel;
            if (folder == null)
                return;
            foreach (var build in folder.Children)
            {
                await TreeTraverseAsync(action, build);
            }
        }

        private static void TreeTraverse<T>(Action<T> action, IBuildViewModel current) where T : class, IBuildViewModel
        {
            var t = current as T;
            if (t != null)
                action(t);
            var folder = current as BuildFolderViewModel;
            folder?.Children.ForEach(b => TreeTraverse(action, b));
        }

        #endregion

        #region Saving and related methods

        private async void BuildOnCollectionChanged(IBuildFolderViewModel build)
        {
            await SaveBuildToFile(build);
            // It's ok that this method doesn't return Task as it is used like an event handler and the
            // async action in SaveBuildToFile does not require to be waited upon.
        }

        private async Task SaveBuildToFile(IBuildViewModel build)
        {
            try
            {
                using (_changingFileSystemMonitor.Enter())
                {
                    PersistentData.SaveBuild(build.Build);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Build save failed for '{build.Build.Name}'", e);
                await _dialogCoordinator.ShowErrorAsync(this,
                    L10n.Message("An error occurred during a save operation."), e.Message);
            }
        }

        private async Task DeleteBuildFile(IBuildViewModel build)
        {
            try
            {
                using (_changingFileSystemMonitor.Enter())
                {
                    PersistentData.DeleteBuild(build.Build);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Build deletion failed for '{build.Build.Name}'", e);
                await _dialogCoordinator.ShowErrorAsync(this,
                    L10n.Message("An error occurred during a delete operation."), e.Message);
            }
        }

        /// <summary>
        /// If there are any unsaved builds the user will be asked if they should be saved. They will be saved if
        /// the user wants to.
        /// </summary>
        /// <param name="message">The message the dialog will show.</param>
        /// <param name="revertWhenUnsaved">True if dirty builds should be reverted to their saved state when the user
        /// doesn't want to save.</param>
        /// <returns>False if the dialog was canceled by the user, true if he clicked Yes or No.</returns>
        public async Task<bool> HandleUnsavedBuilds(string message, bool revertWhenUnsaved)
        {
            var dirtyBuilds = GetDirtyBuilds().ToList();
            if (!dirtyBuilds.Any())
                return true;
            var title = L10n.Message("Unsaved Builds");
            var details = L10n.Message("These builds are not saved:\n");
            foreach (var build in dirtyBuilds)
            {
                details += "\n - " + build.Build.Name;
            }
            var result = await _dialogCoordinator.ShowQuestionAsync(this, message, details, title, MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    foreach (var build in dirtyBuilds)
                    {
                        await SaveBuild(build);
                    }
                    return true;
                case MessageBoxResult.No:
                    if (revertWhenUnsaved)
                    {
                        dirtyBuilds.Where(b => b.Build.CanRevert)
                            .ForEach(b => b.Build.RevertChanges());
                    }
                    return true;
                default:
                    return false;
            }
        }

        private IEnumerable<BuildViewModel> GetDirtyBuilds()
        {
            var dirty = new List<BuildViewModel>();
            TreeTraverse<BuildViewModel>(vm =>
            {
                if (vm.Build.IsDirty)
                    dirty.Add(vm);
            }, BuildRoot);
            return dirty;
        }

        #endregion

        private class CustomDropHandler : DefaultDropHandler
        {
            private readonly BuildsControlViewModel _outer;

            public CustomDropHandler(BuildsControlViewModel outer)
            {
                _outer = outer;
            }

            public override void DragOver(IDropInfo dropInfo)
            {
                base.DragOver(dropInfo);

                // Highlight: insert at TargetItem
                // Insert: insert at TargetItem's parent
                var isHighlight = dropInfo.DropTargetAdorner == DropTargetAdorners.Highlight;

                // Can't drop onto builds
                if (dropInfo.TargetItem is BuildViewModel && isHighlight)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }

                // Ask BuildValidator if drop is possible
                var source = dropInfo.Data as IBuildViewModel;
                if (source == null)
                    return;
                IBuildFolderViewModel target;
                if (isHighlight)
                {
                    target = dropInfo.TargetItem as IBuildFolderViewModel;
                }
                else
                {
                    var targetChild = dropInfo.TargetItem as IBuildViewModel;
                    if (targetChild == null)
                        return;
                    target = targetChild.Parent;
                }
                if (target == null)
                    return;
                if (!_outer._buildValidator.CanMoveTo(source, target))
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
            }
        }
    }
}