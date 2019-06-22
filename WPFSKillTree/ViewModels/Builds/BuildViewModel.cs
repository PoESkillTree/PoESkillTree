using System;
using NLog;
using PoESkillTree.GameModel;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Builds
{
    /// <summary>
    /// <see cref="IBuildViewModel"/> that wraps a <see cref="PoEBuild"/> and therefore represents a leaf build.
    /// </summary>
    public class BuildViewModel : AbstractBuildViewModel<PoEBuild>
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private bool _currentlyOpen;
        private bool _isVisible;
        private CharacterClass _characterClass;
        private string _ascendancyClass;
        private uint _pointsUsed;

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
        /// Gets the character class of the represented tree.
        /// </summary>
        public CharacterClass CharacterClass
        {
            get { return _characterClass; }
            private set { SetProperty(ref _characterClass, value); }
        }

        /// <summary>
        /// Gets the ascendancy class of the represented tree.
        /// </summary>
        public string AscendancyClass
        {
            get { return _ascendancyClass; }
            private set { SetProperty(ref _ascendancyClass, value); }
        }

        /// <summary>
        /// Gets the number of points the represented tree uses.
        /// </summary>
        private uint PointsUsed
        {
            get { return _pointsUsed; }
            set { SetProperty(ref _pointsUsed, value); }
        }

        private string ClassName => AscendancyClass ?? CharacterClass.ToString();

        /// <summary>
        /// Gets the path to a image describing this build.
        /// </summary>
        public string Image
        {
            get
            {
                var imgPath = "/PoESkillTree;component/Images/" + ClassName;
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
                return string.Format(L10n.Plural("{0}, {1} point used", "{0}, {1} points used", PointsUsed),
                    ClassName, PointsUsed);
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
                    case nameof(PoEBuild.TreeUrl):
                        UpdateTreeDependingProperties();
                        break;
                }
                ApplyFilter();
            };
            PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(PointsUsed):
                        OnPropertyChanged(nameof(Description));
                        break;
                    case nameof(CharacterClass):
                        OnPropertyChanged(nameof(Description));
                        OnPropertyChanged(nameof(Image));
                        break;
                    case nameof(AscendancyClass):
                        OnPropertyChanged(nameof(Description));
                        OnPropertyChanged(nameof(Image));
                        break;
                    case nameof(SkillTree):
                        UpdateTreeDependingProperties();
                        break;
                }
                if (args.PropertyName != nameof(IsVisible))
                    ApplyFilter();
            };
        }

        private void UpdateTreeDependingProperties()
        {
            if (SkillTree == null || string.IsNullOrEmpty(Build.TreeUrl))
                return;

            var deserializer = SkillTree.BuildConverter.GetUrlDeserializer(Build.TreeUrl);

            Exception e;
            if (deserializer.ValidateBuildUrl(out e))
            {
                PointsUsed = (uint) deserializer.GetPointsCount();
                CharacterClass = deserializer.GetCharacterClass();
                AscendancyClass = deserializer.GetAscendancyClass();
            }
            else
            {
                Log.Warn(e, $"Could not get tree depending properties for {Build.Name} because the tree is invalid: {Build.TreeUrl}");
                PointsUsed = 0;
            }
        }

        public override void ApplyFilter()
        {
            IsVisible = FilterPredicate(this);
        }
    }
}