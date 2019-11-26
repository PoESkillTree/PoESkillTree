using PoESkillTree.Utils;

namespace PoESkillTree.Model
{
    public class Options : Notifier
    {
        // Don't set language property! When not set, L10n.Initialize will try to use OS settings.
        private string _language = "";
        private string _theme = "Dark";
        private string _accent = "Steel";
        private bool _attributesBarOpened;
        private bool _buildsBarOpened;
        private bool _treeComparisonEnabled;
        private bool _changeSummaryEnabled;
        private bool _showAllAscendancyClasses = true;
        private string _nodeSearchHighlightColor = "Red";
        private string _nodeAttrHighlightColor = "LawnGreen";
        private string _nodeHoverHighlightColor = "DodgerBlue";
        private bool _downloadMissingItemImages = true;
        private ResetPreferences _resetPreferences = ResetPreferences.MainTree | ResetPreferences.AscendancyTree;
        private string _buildSavePath = "";
        private int _loadBuildButtonIndex;

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public string Accent
        {
            get => _accent;
            set => SetProperty(ref _accent, value);
        }

        public bool AttributesBarOpened
        {
            get => _attributesBarOpened;
            set => SetProperty(ref _attributesBarOpened, value);
        }

        public bool BuildsBarOpened
        {
            get => _buildsBarOpened;
            set => SetProperty(ref _buildsBarOpened, value);
        }

        public bool TreeComparisonEnabled
        {
            get => _treeComparisonEnabled;
            set => SetProperty(ref _treeComparisonEnabled, value);
        }

        public bool ChangeSummaryEnabled
        {
            get => _changeSummaryEnabled;
            set => SetProperty(ref _changeSummaryEnabled, value);
        }

        public bool ShowAllAscendancyClasses
        {
            get => _showAllAscendancyClasses;
            set => SetProperty(ref _showAllAscendancyClasses, value);
        }

        public string NodeSearchHighlightColor
        {
            get => _nodeSearchHighlightColor;
            set => SetProperty(ref _nodeSearchHighlightColor, value);
        }

        public string NodeAttrHighlightColor
        {
            get => _nodeAttrHighlightColor;
            set => SetProperty(ref _nodeAttrHighlightColor, value);
        }

        public string NodeHoverHighlightColor
        {
            get => _nodeHoverHighlightColor;
            set => SetProperty(ref _nodeHoverHighlightColor, value);
        }

        public bool DownloadMissingItemImages
        {
            get => _downloadMissingItemImages;
            set => SetProperty(ref _downloadMissingItemImages, value);
        }
        
        public ResetPreferences ResetPreferences
        {
            get => _resetPreferences;
            set => SetProperty(ref _resetPreferences, value);
        }

        public string BuildsSavePath
        {
            get => _buildSavePath;
            set => SetProperty(ref _buildSavePath, value);
        }

        public int LoadTreeButtonIndex
        {
            get => _loadBuildButtonIndex;
            set => SetProperty(ref _loadBuildButtonIndex, value);
        }
    }
}
