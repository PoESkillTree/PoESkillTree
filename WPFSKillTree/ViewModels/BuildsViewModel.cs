using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Utils;
using POESKillTree.Utils.Extensions;
using POESKillTree.Utils.Wpf;

namespace POESKillTree.ViewModels
{
    public interface IBuild : INotifyPropertyChanged, INotifyPropertyChanging
    {
        string Name { get; set; }
    }

    public abstract class AbstractBuild : Notifier, IBuild
    {
        private string _name;

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }

    public class BuildFolder : AbstractBuild
    {
        private bool _isExpanded = true;
        private ObservableCollection<IBuild> _builds = new ObservableCollection<IBuild>();

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        public ObservableCollection<IBuild> Builds
        {
            get { return _builds; }
            set { SetProperty(ref _builds, value); }
        }

        public BuildFolder()
        {
        }

        public BuildFolder(BuildFolder other)
        {
            Name = other.Name;
            IsExpanded = other.IsExpanded;
            foreach (var build in other.Builds)
            {
                if (build is BuildFolder)
                    _builds.Add(new BuildFolder((BuildFolder) build));
                else if (build is Build)
                    _builds.Add(new Build((Build) build));
                else
                    throw new InvalidOperationException("Unsupported build type");
            }
        }
    }

    public class Build : AbstractBuild
    {
        private string _class;
        private uint _pointsUsed;
        private string _note;
        private bool _isDirty;
        private IMemento<Build> _memento;

        public string Class
        {
            get { return _class; }
            set { SetProperty(ref _class, value); }
        }

        public uint PointsUsed
        {
            get { return _pointsUsed; }
            set { SetProperty(ref _pointsUsed, value); }
        }

        public string Note
        {
            get { return _note; }
            set { SetProperty(ref _note, value); }
        }

        public bool IsDirty
        {
            get { return _isDirty; }
            private set { SetProperty(ref _isDirty, value); }
        }

        public Build()
        {
            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(IsDirty):
                        break;
                    default:
                        IsDirty = true;
                        break;
                }
            };
        }

        public Build(Build other)
            : this()
        {
            Name = other.Name;
            Class = other.Class;
            PointsUsed = other.PointsUsed;
            Note = other.Note;
            KeepChanges();
        }

        public void RevertChanges()
        {
            _memento.Restore(this);
            IsDirty = false;
        }

        public void KeepChanges()
        {
            _memento = new Memento(this);
            IsDirty = false;
        }

        public bool CanRevert
        {
            get { return _memento != null; }
        }


        private class Memento : IMemento<Build>
        {
            private readonly string _name;
            private readonly string _class;
            private readonly uint _pointsUsed;
            private readonly string _note;

            public Memento(Build build)
            {
                _name = build.Name;
                _class = build.Class;
                _pointsUsed = build.PointsUsed;
                _note = build.Note;
            }

            public IMemento<Build> Restore(Build target)
            {
                target.Name = _name;
                target.Class = _class;
                target.PointsUsed = _pointsUsed;
                target.Note = _note;
                return this;
            }
        }
    }

    public interface IBuildViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        IBuildFolderViewModel Parent { get; set; }

        bool IsSelected { get; set; }

        IBuild Build { get; }

        void ApplyFilter();
    }

    public interface IBuildViewModel<out T> : IBuildViewModel
        where T : IBuild
    {
        new T Build { get; }
    }

    public interface IBuildFolderViewModel : IBuildViewModel<BuildFolder>
    {
        ObservableCollection<IBuildViewModel> Children { get; }
    }

    public abstract class AbstractBuildViewModel<T> : Notifier, IBuildViewModel<T>
        where T : IBuild
    {
        private IBuildFolderViewModel _parent;
        private bool _isSelected;

        public IBuildFolderViewModel Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public T Build { get; }

        IBuild IBuildViewModel.Build { get { return Build; } }

        protected Predicate<IBuildViewModel> FilterPredicate { get; }

        protected AbstractBuildViewModel(T build, Predicate<IBuildViewModel> filterPredicate)
        {
            Build = build;
            FilterPredicate = filterPredicate;
        }

        public abstract void ApplyFilter();
    }

    public class BuildFolderViewModel : AbstractBuildViewModel<BuildFolder>, IBuildFolderViewModel
    {
        public ObservableCollection<IBuildViewModel> Children { get; } =
            new ObservableCollection<IBuildViewModel>();

        public BuildFolderViewModel(BuildFolder buildFolder, Predicate<IBuildViewModel> filterPredicate)
            : base(buildFolder, filterPredicate)
        {
            CreateChildren();

            Build.PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == nameof(BuildFolder.Builds))
                {
                    Build.Builds.CollectionChanged -= BuildsOnCollectionChanged;
                }
            };
            Build.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(BuildFolder.Builds))
                {
                    Build.Builds.CollectionChanged += BuildsOnCollectionChanged;
                    RecreateChildren();
                }
            };
            Build.Builds.CollectionChanged += BuildsOnCollectionChanged;
        }

        public override void ApplyFilter()
        {
            Children.ForEach(c => c.ApplyFilter());
        }

        private void CreateChildren()
        {
            foreach (var build in Build.Builds)
            {
                if (build is Build)
                    Children.Add(new BuildViewModel((Build)build, FilterPredicate));
                else if (build is BuildFolder)
                    Children.Add(new BuildFolderViewModel((BuildFolder)build, FilterPredicate));
                else
                    throw new InvalidOperationException($"Unknown IBuild implementation {build.GetType()}");
            }
            Children.CollectionChanged += ChildrenOnCollectionChanged;
            SetParents();
        }

        private void RecreateChildren()
        {
            Children.CollectionChanged -= ChildrenOnCollectionChanged;
            UnsetParents();
            Children.Clear();
            CreateChildren();
        }

        private void UnsetParents()
        {
            UnsetParents(Children);
        }
        private void UnsetParents(IEnumerable<IBuildViewModel> builds)
        {
            builds.ForEach(UnsetParent);
        }
        private void UnsetParent(IBuildViewModel build)
        {
            build.Parent = null;
        }

        private void SetParents()
        {
            SetParents(Children);
        }
        private void SetParents(IEnumerable<IBuildViewModel> builds)
        {
            builds.ForEach(SetParent);
        }
        private void SetParent(IBuildViewModel build)
        {
            build.Parent = this;
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var newBuilds = notifyCollectionChangedEventArgs.NewItems?.Cast<IBuildViewModel>();
            var oldBuilds = notifyCollectionChangedEventArgs.OldItems?.Cast<IBuildViewModel>();
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    SetParents(newBuilds);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UnsetParents(oldBuilds);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    SetParents(newBuilds);
                    UnsetParents(oldBuilds);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Can't access the old items here so they may have invalid Parent values.
                    SetParents();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IBuildViewModel GetChildForBuild(IBuild build)
        {
            var ret = Children.FirstOrDefault(vm => vm.Build == build);
            if (ret != null) return ret;

            if (build is Build)
                return new BuildViewModel((Build)build, FilterPredicate);
            if (build is BuildFolder)
                return new BuildFolderViewModel((BuildFolder)build, FilterPredicate);
            return null;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException", Justification = "Only null when not accessed")]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "Only null when not accessed")]
        private void BuildsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var newBuilds = notifyCollectionChangedEventArgs.NewItems?.Cast<IBuild>();
            var oldBuilds = notifyCollectionChangedEventArgs.OldItems?.Cast<IBuild>();
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var newBuild in newBuilds)
                    {
                        var vm = GetChildForBuild(newBuild);
                        SetParent(vm);
                        if (i < 0)
                        {
                            Children.Add(vm);
                        }
                        else
                        {
                            Children.Insert(i, vm);
                            i++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldBuild in oldBuilds)
                    {
                        var vm = GetChildForBuild(oldBuild);
                        UnsetParent(vm);
                        Children.Remove(vm);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (notifyCollectionChangedEventArgs.NewStartingIndex < 0)
                    {
                        foreach (var oldBuild in oldBuilds)
                        {
                            var vm = GetChildForBuild(oldBuild);
                            UnsetParent(vm);
                            Children.Remove(vm);
                        }
                        foreach (var newBuild in newBuilds)
                        {
                            var vm = GetChildForBuild(newBuild);
                            SetParent(vm);
                            Children.Add(vm);
                        }

                    }
                    else
                    {
                        var start = notifyCollectionChangedEventArgs.NewStartingIndex;
                        var oldList = oldBuilds.ToList();
                        var newList = newBuilds.ToList();
                        for (var j = 0; j < newList.Count; j++)
                        {
                            var oldVm = GetChildForBuild(oldList[j]);
                            var newVm = GetChildForBuild(newList[j]);
                            UnsetParent(oldVm);
                            SetParent(newVm);
                            Children.RemoveAt(start + j);
                            Children.Insert(start + j, newVm);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var oldBuild in oldBuilds)
                    {
                        Children.Remove(GetChildForBuild(oldBuild));
                    }
                    i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var newBuild in newBuilds)
                    {
                        Children.Insert(i, GetChildForBuild(newBuild));
                        i++;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    RecreateChildren();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class BuildViewModel : AbstractBuildViewModel<Build>
    {
        private bool _currentlyOpen;
        private bool _isVisible;

        public bool CurrentlyOpen
        {
            get { return _currentlyOpen; }
            set { SetProperty(ref _currentlyOpen, value, () => OnPropertyChanged(nameof(Image))); }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set { SetProperty(ref _isVisible, value); }
        }

        public string Image
        {
            get
            {
                var imgPath = "/POESKillTree;component/Images/" + Build.Class;
                if (CurrentlyOpen)
                    imgPath += "_Highlighted";
                return imgPath + ".jpg";
            }
        }

        public string Description
        {
            get
            {
                return string.Format(L10n.Plural("{0}, {1} point used", "{0}, {1} points used", Build.PointsUsed),
                    Build.Class, Build.PointsUsed);
            }
        }

        public BuildViewModel(Build build, Predicate<IBuildViewModel> filterPredicate) : base(build, filterPredicate)
        {
            IsVisible = FilterPredicate(this);
            build.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(Build.PointsUsed):
                    case nameof(Build.Class):
                        OnPropertyChanged(nameof(Description));
                        break;
                }
                ApplyFilter();
            };
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(IsVisible))
                    ApplyFilter();
            };
        }

        public override void ApplyFilter()
        {
            IsVisible = FilterPredicate(this);
        }
    }

    public class BuildsViewModel : Notifier
    {
        private readonly BuildFolder _buildModels = new BuildFolder
        {
            Name = "Root", Builds = new ObservableCollection<IBuild>
            {
                new BuildFolder
                {
                    Name = "Juggernaut Mjölner", Builds = new ObservableCollection<IBuild>
                    {
                        new Build
                        {
                            Name = "RT", Class = "Marauder", PointsUsed = 115
                        },
                        new Build
                        {
                            Name = "Crit", Class = "Marauder", PointsUsed = 113
                        }
                    }
                },
                new BuildFolder
                {
                    Name = "Bow", Builds = new ObservableCollection<IBuild>
                    {
                        new Build
                        {
                            Name = "Windripper", Class = "Ranger", PointsUsed = 110
                        },
                        new BuildFolder
                        {
                            Name = "Physical", IsExpanded = false, Builds = new ObservableCollection<IBuild>
                            {
                                new Build
                                {
                                    Name = "Pathfinder", Class = "Ranger", PointsUsed = 117
                                }
                            }
                        }
                    }
                },
                new Build
                {
                    Name = "Some weird experiment", Class = "Templar", PointsUsed = 10, Note = "This build is not good"
                }
            }
        };

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

        public bool IsSkillTreeComparisionEnabled { get; set; }

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

        public BuildsViewModel(IDialogCoordinator dialogCoordinator)
        {
            BuildRoot = new BuildFolderViewModel(_buildModels, Filter);
            ((BuildViewModel) ((IBuildFolderViewModel) BuildRoot.Children[0]).Children[1]).CurrentlyOpen = true;
            ((IBuildFolderViewModel) BuildRoot.Children[1]).Children[0].IsSelected = true;
            TreeTraverse<Build>(build => build.KeepChanges(), _buildModels);

            _currentBuild = TreeFind<BuildViewModel>(b => b.CurrentlyOpen, BuildRoot);
            _selectedBuild = TreeFind<IBuildViewModel>(b => b.IsSelected, BuildRoot);

            NewFolderCommand = new RelayCommand<IBuildFolderViewModel>(async folder =>
            {
                var name = await dialogCoordinator.ShowInputAsync(this, L10n.Message("New Folder"),
                    L10n.Message("Enter the name of the new folder"));
                if (string.IsNullOrWhiteSpace(name))
                    return;
                var newFolder = new BuildFolderViewModel(new BuildFolder {Name = name}, Filter);
                folder.Children.Add(newFolder);
            });
            NewBuildCommand = new RelayCommand<IBuildFolderViewModel>(async folder =>
            {
                var name = await dialogCoordinator.ShowInputAsync(this, L10n.Message("New Build"),
                    L10n.Message("Enter the name of the new build"));
                if (string.IsNullOrWhiteSpace(name))
                    return;
                var build = new BuildViewModel(new Build {Name = name, Class = "Ranger"}, Filter);
                folder.Children.Add(build);
            });
            DeleteCommand = new RelayCommand<IBuildViewModel>(async build =>
            {
                if (TreeFind<BuildViewModel>(b => b == CurrentBuild, build) != null)
                {
                    await dialogCoordinator.ShowInfoAsync(this,
                        L10n.Message("The currently opened build can not be deleted."));
                    return;
                }
                if (build is IBuildFolderViewModel)
                {
                    var result = await dialogCoordinator.ShowQuestionAsync(this,
                        string.Format(L10n.Message("This will delete the build folder \"{0}\" and all its contents.\n"),
                            build.Build.Name) + L10n.Message("Do you want to continue?"));
                    if (result != MessageBoxResult.Yes)
                        return;
                }
                build.Parent.Children.Remove(build);
            }, o => o != BuildRoot);
            OpenBuildCommand = new RelayCommand<BuildViewModel>(build => CurrentBuild = build);
            SaveBuildCommand = new RelayCommand<BuildViewModel>(build => build.Build.KeepChanges(),
                b => b != null && b.Build.IsDirty);
            SaveBuildAsCommand = new RelayCommand<BuildViewModel>(async vm =>
            {
                var build = vm.Build;
                var name = await dialogCoordinator.ShowInputAsync(this, L10n.Message("Save as"),
                    L10n.Message("Enter the new name of the build"), build.Name);
                if (string.IsNullOrWhiteSpace(name))
                    return;
                var newBuild = new Build(build) {Name = name};
                newBuild.KeepChanges();
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
            });
            SaveAllBuildsCommand = new RelayCommand(
                _ => TreeTraverse<BuildViewModel>(build => build.Build.KeepChanges(), BuildRoot),
                _ => TreeFind<BuildViewModel>(b => b.Build.IsDirty, BuildRoot) != null);
            RevertBuildCommand = new RelayCommand<BuildViewModel>(build => build.Build.RevertChanges(),
                b => b != null && b.Build.IsDirty && b.Build.CanRevert);
            MoveUpCommand = new RelayCommand<IBuildViewModel>(build =>
            {
                var list = build.Parent.Children;
                var i = list.IndexOf(build);
                list.Move(i, i - 1);
            }, o => o != BuildRoot && o.Parent.Children.IndexOf(o) > 0);
            MoveDownCommand = new RelayCommand<IBuildViewModel>(build =>
            {
                var list = build.Parent.Children;
                var i = list.IndexOf(build);
                list.Move(i, i + 1);
            }, o => o != BuildRoot && o.Parent.Children.IndexOf(o) < o.Parent.Children.Count - 1);
            EditCommand = new RelayCommand<IBuildViewModel>(async build =>
            {
                // todo for folder: open EditFolderWindow
                // todo for build: open EditBuildWindow
                var name = await dialogCoordinator.ShowInputAsync(this, L10n.Message("Edit Build"),
                    L10n.Message("Enter the new name for this build below.")
                    + L10n.Message("\nImagine this dialog being like the known edit build dialog."),
                    build.Build.Name);
                if (string.IsNullOrWhiteSpace(name))
                    return;
                build.Build.Name = name;
            });
            CutCommand = new RelayCommand<IBuildViewModel>(async b =>
            {
                b.IsSelected = false;
                b.Parent.IsSelected = true;
                await Task.Delay(1);
                b.Parent.Children.Remove(b);
                _buildClipboard = b;
                _clipboardIsCopy = false;
            }, b => b != BuildRoot);
            CopyCommand = new RelayCommand<IBuildViewModel>(b =>
            {
                _buildClipboard = b;
                _clipboardIsCopy = true;
            });
            PasteCommand = new RelayCommand<IBuildFolderViewModel>(b =>
            {
                IBuildViewModel pasted;
                if (_clipboardIsCopy)
                {
                    var folder = _buildClipboard as IBuildViewModel<BuildFolder>;
                    if (folder == null)
                    {
                        var vm = (IBuildViewModel<Build>) _buildClipboard;
                        pasted = new BuildViewModel(new Build(vm.Build), Filter);
                    }
                    else
                    {
                        pasted = new BuildFolderViewModel(new BuildFolder(folder.Build), Filter);
                    }
                }
                else
                {
                    pasted = _buildClipboard;
                    _buildClipboard = null;
                }
                b.Children.Add(pasted);
            }, _ => _buildClipboard != null);
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

        private static void TreeTraverse<T>(Action<T> action, IBuild current) where T : class, IBuild
        {
            var t = current as T;
            if (t != null)
                action(t);
            var folder = current as BuildFolder;
            folder?.Builds.ForEach(build => TreeTraverse(action, build));
        }

        private static void TreeTraverse<T>(Action<T> action, IBuildViewModel current) where T : class, IBuildViewModel
        {
            var t = current as T;
            if (t != null)
                action(t);
            var folder = current as BuildFolderViewModel;
            folder?.Children.ForEach(build => TreeTraverse(action, build));
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

    public class BuildsViewModelProxy : BindingProxy<BuildsViewModel>
    {
    }
}