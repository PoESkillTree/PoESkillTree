using System;
using POESKillTree.Common;
using POESKillTree.Model.Builds;
using POESKillTree.Utils;

namespace POESKillTree.ViewModels.Builds
{
    /// <summary>
    /// Abstract implementation of <see cref="IBuildViewModel{T}"/> that extends <see cref="Notifier"/>.
    /// </summary>
    public abstract class AbstractBuildViewModel<T> : Notifier, IBuildViewModel<T>
        where T : IBuild
    {
        private IBuildFolderViewModel _parent;
        private bool _isSelected;
        private ISkillTree _skillTree;

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

        public ISkillTree SkillTree
        {
            protected get { return _skillTree; }
            set { SetProperty(ref _skillTree, value); }
        }

        IBuild IBuildViewModel.Build { get { return Build; } }

        /// <summary>
        /// Gets a predicate that returns whether the given <see cref="IBuildViewModel"/> should be filtered or not.
        /// </summary>
        protected Predicate<IBuildViewModel> FilterPredicate { get; }

        /// <param name="build">The wrapped build.</param>
        /// <param name="filterPredicate">A predicate that returns whether the given <see cref="IBuildViewModel"/>
        /// should be filtered or not.</param>
        protected AbstractBuildViewModel(T build, Predicate<IBuildViewModel> filterPredicate)
        {
            Build = build;
            FilterPredicate = filterPredicate;
        }

        public abstract void ApplyFilter();
    }
}