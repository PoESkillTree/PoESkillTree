using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public class Options : Notifier
    {
        // Don't set language property! When not set, L10n.Initialize will try to use OS settings.
        private string _language;
        private string _theme = "Dark";
        private string _accent = "Steel";
        private bool _attributesBarOpened;
        private bool _characterSheetBarOpened;
        private bool _buildsBarOpened;
        private bool _treeComparisonEnabled;
        private bool _showAllAscendancyClasses = true;
        private string _nodeSearchHighlightColor = "Red";
        private string _nodeAttrHighlightColor = "LawnGreen";
        private string _nodeHoverHighlightColor = "DodgerBlue";
        private bool _downloadMissingItemImages;
        private ResetPreferences _resetPreferences = ResetPreferences.MainTree | ResetPreferences.AscendancyTree;
        private string _buildSavePath;
        private int _loadBuildButtonIndex;

        public string Language
        {
            get { return _language; }
            set { SetProperty(ref _language, value); }
        }

        public string Theme
        {
            get { return _theme; }
            set { SetProperty(ref _theme, value); }
        }

        public string Accent
        {
            get { return _accent; }
            set { SetProperty(ref _accent, value); }
        }

        public bool AttributesBarOpened
        {
            get { return _attributesBarOpened; }
            set { SetProperty(ref _attributesBarOpened, value); }
        }

        public bool CharacterSheetBarOpened
        {
            get { return _characterSheetBarOpened; }
            set { SetProperty(ref _characterSheetBarOpened, value); }
        }

        public bool BuildsBarOpened
        {
            get { return _buildsBarOpened; }
            set { SetProperty(ref _buildsBarOpened, value); }
        }

        public bool TreeComparisonEnabled
        {
            get { return _treeComparisonEnabled; }
            set { SetProperty(ref _treeComparisonEnabled, value); }
        }

        public bool ShowAllAscendancyClasses
        {
            get { return _showAllAscendancyClasses; }
            set { SetProperty(ref _showAllAscendancyClasses, value); }
        }

        public string NodeSearchHighlightColor
        {
            get { return _nodeSearchHighlightColor; }
            set { SetProperty(ref _nodeSearchHighlightColor, value); }
        }

        public string NodeAttrHighlightColor
        {
            get { return _nodeAttrHighlightColor; }
            set { SetProperty(ref _nodeAttrHighlightColor, value); }
        }

        public string NodeHoverHighlightColor
        {
            get { return _nodeHoverHighlightColor; }
            set { SetProperty(ref _nodeHoverHighlightColor, value); }
        }

        public bool DownloadMissingItemImages
        {
            get { return _downloadMissingItemImages; }
            set { SetProperty(ref _downloadMissingItemImages, value); }
        }
        
        public ResetPreferences ResetPreferences
        {
            get { return _resetPreferences; }
            set { SetProperty(ref _resetPreferences, value); }
        }

        public string BuildsSavePath
        {
            get { return _buildSavePath; }
            set { SetProperty(ref _buildSavePath, value); }
        }

        public int LoadTreeButtonIndex
        {
            get { return _loadBuildButtonIndex; }
            set { SetProperty(ref _loadBuildButtonIndex, value); }
        }
    }
}
