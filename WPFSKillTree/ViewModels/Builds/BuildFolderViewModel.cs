using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using POESKillTree.Model;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.ViewModels.Builds
{
    public class BuildFolderViewModel : AbstractBuildViewModel<BuildFolder>, IBuildFolderViewModel
    {
        public ObservableCollection<IBuildViewModel> Children { get; } =
            new ObservableCollection<IBuildViewModel>();

        private bool _isInCollectionChanged;

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
            if (_isInCollectionChanged)
                return;
            _isInCollectionChanged = true;
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                SetParents();
                Build.Builds.Clear();
                Children.Select(c => c.Build).ForEach(b => Build.Builds.Add(b));
            }
            else
            {
                CollectionChanged<IBuildViewModel, IBuild>(notifyCollectionChangedEventArgs, Build.Builds, b => b.Build);
            }
            _isInCollectionChanged = false;
        }
        
        private void BuildsOnCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (_isInCollectionChanged)
                return;
            _isInCollectionChanged = true;
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                RecreateChildren();
            }
            else
            {
                CollectionChanged<IBuild, IBuildViewModel>(notifyCollectionChangedEventArgs, Children, GetChildForBuild);
            }
            _isInCollectionChanged = false;
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

        private void CollectionChanged<TSource, TTarget>(
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs,
            ObservableCollection<TTarget> targetCollection, Func<TSource, TTarget> toTargetFunc)
        {
            Func<TSource, IBuildViewModel> toVmFunc;
            if (typeof(TSource) == typeof(IBuildViewModel))
                toVmFunc = o => (IBuildViewModel) o;
            else if (typeof(TTarget) == typeof(IBuildViewModel))
                toVmFunc = o => (IBuildViewModel) toTargetFunc(o);
            else
                throw new ArgumentException("One type parameter must be IBuildViewModel");
            var newBuilds = notifyCollectionChangedEventArgs.NewItems?.Cast<TSource>() ?? new TSource[0];
            var oldBuilds = notifyCollectionChangedEventArgs.OldItems?.Cast<TSource>() ?? new TSource[0];
            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var newBuild in newBuilds)
                    {
                        var t = toTargetFunc(newBuild);
                        SetParent(toVmFunc(newBuild));
                        if (i < 0)
                        {
                            targetCollection.Add(t);
                        }
                        else
                        {
                            targetCollection.Insert(i, t);
                            i++;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldBuild in oldBuilds)
                    {
                        UnsetParent(toVmFunc(oldBuild));
                        targetCollection.Remove(toTargetFunc(oldBuild));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (notifyCollectionChangedEventArgs.NewStartingIndex < 0)
                    {
                        foreach (var oldBuild in oldBuilds)
                        {
                            UnsetParent(toVmFunc(oldBuild));
                            targetCollection.Remove(toTargetFunc(oldBuild));
                        }
                        foreach (var newBuild in newBuilds)
                        {
                            SetParent(toVmFunc(newBuild));
                            targetCollection.Add(toTargetFunc(newBuild));
                        }
                    }
                    else
                    {
                        var start = notifyCollectionChangedEventArgs.NewStartingIndex;
                        var oldList = oldBuilds.ToList();
                        var newList = newBuilds.ToList();
                        var minCount = Math.Min(newList.Count, oldList.Count);
                        for (var j = 0; j < minCount; j++)
                        {
                            UnsetParent(toVmFunc(oldList[j]));
                            SetParent(toVmFunc(newList[j]));
                            targetCollection.RemoveAt(start + j);
                            targetCollection.Insert(start + j, toTargetFunc(newList[j]));
                        }
                        for (var j = minCount; j < oldList.Count; j++)
                        {
                            UnsetParent(toVmFunc(oldList[j]));
                            targetCollection.Remove(toTargetFunc(oldList[j]));
                        }
                        for (var j = minCount; j < newList.Count; j++)
                        {
                            SetParent(toVmFunc(newList[j]));
                            targetCollection.Insert(start + j, toTargetFunc(newList[j]));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    foreach (var oldBuild in oldBuilds)
                    {
                        targetCollection.Remove(toTargetFunc(oldBuild));
                    }
                    i = notifyCollectionChangedEventArgs.NewStartingIndex;
                    foreach (var newBuild in newBuilds)
                    {
                        targetCollection.Insert(i, toTargetFunc(newBuild));
                        i++;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}