using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using POESKillTree.Model;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.ViewModels.Builds
{
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
                if (build is PoEBuild)
                    Children.Add(new BuildViewModel((PoEBuild)build, FilterPredicate));
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

            if (build is PoEBuild)
                return new BuildViewModel((PoEBuild)build, FilterPredicate);
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
}