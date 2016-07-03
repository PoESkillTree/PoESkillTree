using System;
using POESKillTree.Model;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Builds
{
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

        protected AbstractBuildViewModel(T poEBuild, Predicate<IBuildViewModel> filterPredicate)
        {
            Build = poEBuild;
            FilterPredicate = filterPredicate;
        }

        public abstract void ApplyFilter();
    }
}