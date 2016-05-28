using System.ComponentModel;
using POESKillTree.Utils;

namespace POESKillTree.Model
{
    public interface IOptions : INotifyPropertyChanged, INotifyPropertyChanging
    {
        string Language { get; set; }
        bool AttributesBarOpened { get; set; }
        bool CharacterSheetBarOpened { get; set; }
        bool BuildsBarOpened { get; set; }
        bool TreeComparisonEnabled { get; set; }
        int SelectedBuildIndex { get; set; }
        string NodeSearchHighlightColor { get; set; }
        string NodeHoverHighlightColor { get; set; }
        string NodeAttrHighlightColor { get; set; }
        bool ShowAllAscendancyClasses { get; set; }
        bool DownloadMissingItemImages { get; set; }
        ResetPreferences ResetPreferences { get; set; }
    }

    public class Options : Notifier, IOptions
    {
        private string _language;
        public string Language
        {
            get { return _language; }
            set { SetProperty(ref _language, value); }
        }

        public string Theme { get; set; }
        public string Accent { get; set; } //Controlled by Menu Accent Headers

        private bool _attributesBarOpened;
        public bool AttributesBarOpened
        {
            get { return _attributesBarOpened; }
            set { SetProperty(ref _attributesBarOpened, value); }
        }

        private bool _characterSheetBarOpened;
        public bool CharacterSheetBarOpened
        {
            get { return _characterSheetBarOpened; }
            set { SetProperty(ref _characterSheetBarOpened, value); }
        }

        private bool _buildsBarOpened;
        public bool BuildsBarOpened
        {
            get { return _buildsBarOpened; }
            set { SetProperty(ref _buildsBarOpened, value); }
        }

        private bool _treeComparisonEnabled;
        public bool TreeComparisonEnabled
        {
            get { return _treeComparisonEnabled; }
            set { SetProperty(ref _treeComparisonEnabled, value); }
        }

        private int _selectedBuildIndex = -1;
        public int SelectedBuildIndex
        {
            get { return _selectedBuildIndex; }
            set { SetProperty(ref _selectedBuildIndex, value); }
        }

        private bool _showAllAscendancyClasses = true;
        public bool ShowAllAscendancyClasses
        {
            get { return _showAllAscendancyClasses; }
            set { SetProperty(ref _showAllAscendancyClasses, value); }
        }

        private string _nodeSearchHighlightColor = "Red";
        public string NodeSearchHighlightColor
        {
            get { return _nodeSearchHighlightColor; }
            set { SetProperty(ref _nodeSearchHighlightColor, value); }
        }

        private string _nodeAttrHighlightColor = "LawnGreen";
        public string NodeAttrHighlightColor
        {
            get { return _nodeAttrHighlightColor; }
            set { SetProperty(ref _nodeAttrHighlightColor, value); }
        }

        private string _nodeHoverHighlightColor = "DodgerBlue";
        public string NodeHoverHighlightColor
        {
            get { return _nodeHoverHighlightColor; }
            set { SetProperty(ref _nodeHoverHighlightColor, value); }
        }

        private bool _downloadMissingItemImages = true;
        public bool DownloadMissingItemImages
        {
            get { return _downloadMissingItemImages; }
            set { SetProperty(ref _downloadMissingItemImages, value); }
        }

        private ResetPreferences _resetPreferences = ResetPreferences.MainTree | ResetPreferences.AscendancyTree;
        public ResetPreferences ResetPreferences
        {
            get { return _resetPreferences; }
            set { SetProperty(ref _resetPreferences, value); }
        }

        public Options()
        {
            // Don't set Language property! When not set, L10n.Initialize will try to use OS settings.
            Theme = "Dark";
            Accent = "Steel";
        }
    }
}
