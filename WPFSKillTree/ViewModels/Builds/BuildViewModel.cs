using System;
using POESKillTree.Localization;
using POESKillTree.Model.Builds;

namespace POESKillTree.ViewModels.Builds
{
    /// <summary>
    /// <see cref="IBuildViewModel"/> that wraps a <see cref="PoEBuild"/> and therefore represents a leaf build.
    /// </summary>
    public class BuildViewModel : AbstractBuildViewModel<PoEBuild>
    {
        private bool _currentlyOpen;
        private bool _isVisible;

        /// <summary>
        /// Gets or sets whether this is the currently opened build.
        /// </summary>
        public bool CurrentlyOpen
        {
            get { return _currentlyOpen; }
            set { SetProperty(ref _currentlyOpen, value, () => OnPropertyChanged(nameof(Image))); }
        }

        /// <summary>
        /// Gets whether this build should be visible in the UI.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            private set { SetProperty(ref _isVisible, value); }
        }

        /// <summary>
        /// Gets the path to a image describing this build.
        /// </summary>
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

        /// <summary>
        /// Gets a description of this build.
        /// </summary>
        public string Description
        {
            get
            {
                return string.Format(L10n.Plural("{0}, {1} point used", "{0}, {1} points used", Build.PointsUsed),
                    Build.Class, Build.PointsUsed);
            }
        }

        /// <param name="poEBuild">The wrapped build.</param>
        /// <param name="filterPredicate">A predicate that returns whether the given <see cref="IBuildViewModel"/>
        /// should be filtered or not.</param>
        public BuildViewModel(PoEBuild poEBuild, Predicate<IBuildViewModel> filterPredicate) : base(poEBuild, filterPredicate)
        {
            IsVisible = FilterPredicate(this);
            poEBuild.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PoEBuild.PointsUsed):
                        OnPropertyChanged(nameof(Description));
                        break;
                    case nameof(PoEBuild.Class):
                        OnPropertyChanged(nameof(Description));
                        OnPropertyChanged(nameof(Image));
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
}