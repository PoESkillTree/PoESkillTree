using System;
using POESKillTree.Localization;
using POESKillTree.Model;

namespace POESKillTree.ViewModels.Builds
{
    public class BuildViewModel : AbstractBuildViewModel<PoEBuild>
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

        public BuildViewModel(PoEBuild poEBuild, Predicate<IBuildViewModel> filterPredicate) : base(poEBuild, filterPredicate)
        {
            IsVisible = FilterPredicate(this);
            poEBuild.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PoEBuild.PointsUsed):
                    case nameof(PoEBuild.Class):
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
}