using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.Wpf;
using POESKillTree.ViewModels.Builds;

namespace POESKillTree.ViewModels
{
    public class BuildsViewModelProxy : BindingProxy<BuildsControlViewModel>
    {
    }

    public class BuildsControlViewModel : Notifier
    {
        private readonly IExtendedDialogCoordinator _dialogCoordinator;

        public IBuildFolderViewModel BuildRoot { get; }

        public IDropTarget DropHandler { get; } = new CustomDropHandler();

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

        public IPersistentData PersistentData { get; }

        private BuildViewModel _currentBuild;
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
        public IBuildViewModel SelectedBuild
        {
            get { return _selectedBuild; }
            set
            {
                SetProperty(ref _selectedBuild, value, () =>
                {
                    if (SelectedBuild != null)
                        SelectedBuild.IsSelected = true;
                    PersistentData.SelectedBuild = SelectedBuild?.Build as PoEBuild;
                }, b =>
                {
                    if (SelectedBuild != null)
                        SelectedBuild.IsSelected = false;
                });
            }
        }

        private string _classFilter = "All";
        public string ClassFilter
        {
            get { return _classFilter; }
            set { SetProperty(ref _classFilter, value, () => BuildRoot.ApplyFilter()); }
        }

        private string _textFilter;
        public string TextFilter
        {
            get { return _textFilter; }
            set { SetProperty(ref _textFilter, value, () => BuildRoot.ApplyFilter()); }
        }

        private IBuildViewModel _buildClipboard;
        private bool _clipboardIsCopy;

        public BuildsControlViewModel(IExtendedDialogCoordinator dialogCoordinator, IPersistentData persistentData)
        {
            _dialogCoordinator = dialogCoordinator;
            PersistentData = persistentData;
            BuildRoot = new BuildFolderViewModel(persistentData.RootBuild, Filter);

            CurrentBuild = TreeFind<BuildViewModel>(b => b.Build == PersistentData.CurrentBuild, BuildRoot);
            SelectedBuild = TreeFind<IBuildViewModel>(b => b.Build == PersistentData.SelectedBuild, BuildRoot);

            NewFolderCommand = new RelayCommand<IBuildFolderViewModel>(
                async b => await NewFolder(b));
            NewBuildCommand = new RelayCommand<IBuildFolderViewModel>(
                async b => await NewBuild(b));
            DeleteCommand = new RelayCommand<IBuildViewModel>(
                async b => await Delete(b),
                o => o != BuildRoot);
            OpenBuildCommand = new RelayCommand<BuildViewModel>(
                build => CurrentBuild = build);
            SaveBuildCommand = new RelayCommand<BuildViewModel>(
                async b => await SaveBuild(b),
                b => b != null && b.Build.IsDirty);
            SaveBuildAsCommand = new RelayCommand<BuildViewModel>(
                async b => await SaveBuildAs(b));
            SaveAllBuildsCommand = new RelayCommand(
                _ => TreeTraverse<BuildViewModel>(async build => await SaveBuildToFile(build), BuildRoot),
                _ => TreeFind<BuildViewModel>(b => b.Build.IsDirty, BuildRoot) != null);
            RevertBuildCommand = new RelayCommand<BuildViewModel>(
                build => build.Build.RevertChanges(),
                b => b != null && b.Build.IsDirty && b.Build.CanRevert);
            MoveUpCommand = new RelayCommand<IBuildViewModel>(
                MoveUp,
                o => o != BuildRoot && o.Parent.Children.IndexOf(o) > 0);
            MoveDownCommand = new RelayCommand<IBuildViewModel>(
                MoveDown,
                o => o != BuildRoot && o.Parent.Children.IndexOf(o) < o.Parent.Children.Count - 1);
            EditCommand = new RelayCommand<IBuildViewModel>(
                async build => await Edit(build));
            CutCommand = new RelayCommand<IBuildViewModel>(
                Cut,
                b => b != BuildRoot);
            CopyCommand = new RelayCommand<IBuildViewModel>(
                Copy);
            PasteCommand = new RelayCommand<IBuildFolderViewModel>(
                Paste,
                _ => _buildClipboard != null);
        }

        private async Task NewFolder(IBuildFolderViewModel folder)
        {
            var name = await _dialogCoordinator.ShowInputAsync(this, L10n.Message("New Folder"),
                L10n.Message("Enter the name of the new folder"));
            if (string.IsNullOrWhiteSpace(name))
                return;
            var newFolder = new BuildFolderViewModel(new BuildFolder { Name = name }, Filter);
            folder.Children.Add(newFolder);
        }

        public async Task NewBuild(IBuildFolderViewModel folder)
        {
            var name = await _dialogCoordinator.ShowInputAsync(this, L10n.Message("New Build"),
                L10n.Message("Enter the name of the new build"));
            if (string.IsNullOrWhiteSpace(name))
                return;
            var build = new BuildViewModel(new PoEBuild { Name = name, Class = "Ranger" }, Filter);
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
        }

        public async Task SaveBuild(BuildViewModel build)
        {
            build.Build.LastUpdated = DateTime.Now;
            await SaveBuildToFile(build);
        }

        public async Task SaveBuildAs(BuildViewModel vm)
        {
            var build = vm.Build;
            var name = await _dialogCoordinator.ShowInputAsync(this, L10n.Message("Save as"),
                L10n.Message("Enter the new name of the build"), build.Name);
            if (string.IsNullOrWhiteSpace(name))
                return;
            var newBuild = build.DeepClone();
            newBuild.Name = name;
            newBuild.LastUpdated = DateTime.Now;
            var newVm = new BuildViewModel(newBuild, Filter);
            await SaveBuildToFile(newVm);

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

        private void Cut(IBuildViewModel build)
        {
            build.IsSelected = false;
            build.Parent.IsSelected = true;
            build.Parent.Children.Remove(build);
            _buildClipboard = build;
            _clipboardIsCopy = false;
        }

        private void Copy(IBuildViewModel build)
        {
            _buildClipboard = build;
            _clipboardIsCopy = true;
        }

        private void Paste(IBuildFolderViewModel build)
        {
            IBuildViewModel pasted;
            if (_clipboardIsCopy)
            {
                var folder = _buildClipboard as IBuildViewModel<BuildFolder>;
                if (folder == null)
                {
                    var vm = (IBuildViewModel<PoEBuild>)_buildClipboard;
                    pasted = new BuildViewModel(vm.Build.DeepClone(), Filter);
                }
                else
                {
                    pasted = new BuildFolderViewModel(folder.Build.DeepClone(), Filter);
                }
            }
            else
            {
                pasted = _buildClipboard;
                _buildClipboard = null;
            }
            build.Children.Add(pasted);
        }

        private async Task Edit(IBuildViewModel build)
        {

            var buildVm = build as IBuildViewModel<PoEBuild>;
            var folderVm = build as IBuildViewModel<BuildFolder>;
            if (buildVm != null)
            {
                if (await _dialogCoordinator.EditBuildAsync(this, buildVm.Build))
                {
                    buildVm.Build.LastUpdated = DateTime.Now;
                }
            }
            else if (folderVm != null)
            {
                await _dialogCoordinator.EditFolderAsync(this, folderVm.Build);
            }
            else
            {
                throw new ArgumentException("Argument's IBuild implementation is not supported");
            }
        }

        private bool Filter(IBuildViewModel b)
        {
            var build = b as BuildViewModel;
            if (build == null)
                return true;
            if (!string.IsNullOrEmpty(ClassFilter) && ClassFilter != "All"
                && build.Build.Class != ClassFilter)
            {
                return false;
            }
            if (!string.IsNullOrEmpty(TextFilter)
                && !build.Build.Name.Contains(TextFilter, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            return true;
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

        private static void TreeTraverse<T>(Action<T> action, IBuildViewModel current) where T : class, IBuildViewModel
        {
            var t = current as T;
            if (t != null)
                action(t);
            var folder = current as BuildFolderViewModel;
            folder?.Children.ForEach(build => TreeTraverse(action, build));
        }

        private async Task SaveBuildToFile(IBuildViewModel build)
        {
            // todo Use single file saving
            // todo Call on all folder changes
            // todo Should maybe be done in PersistentData
            try
            {
                PersistentData.SaveToFile();
                (build.Build as PoEBuild)?.KeepChanges();
            }
            catch (Exception e)
            {
                await _dialogCoordinator.ShowErrorAsync(this, L10n.Message("An error occurred during a save operation."), e.Message);
            }
        }

        private class CustomDropHandler : DefaultDropHandler
        {
            public override void DragOver(IDropInfo dropInfo)
            {
                base.DragOver(dropInfo);
                if (dropInfo.TargetItem is BuildViewModel && dropInfo.DropTargetAdorner == DropTargetAdorners.Highlight)
                {
                    dropInfo.Effects = DragDropEffects.None;
                }
            }
        }
    }
}