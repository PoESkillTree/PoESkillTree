using System.ComponentModel;
using POESKillTree.Localization;

namespace POESKillTree.Model
{
    public class Options : INotifyPropertyChanged
    {
        public string Language { get; set; }
        public string Theme { get; set; }
        public string Accent { get; set; } //Controlled by Menu Accent Headers

        private bool _attributesBarOpened;
        public bool AttributesBarOpened
        {
            get { return _attributesBarOpened; }
            set
            {
                _attributesBarOpened = value;
                OnPropertyChanged("AttributesBarOpened");
            }
        }

        private bool _characterSheetBarOpened;
        public bool CharacterSheetBarOpened
        {
            get { return _characterSheetBarOpened; }
            set
            {
                _characterSheetBarOpened = value;
                OnPropertyChanged("CharacterSheetBarOpened");
            }
        }

        private bool _buildsBarOpened;
        public bool BuildsBarOpened
        {
            get { return _buildsBarOpened; }
            set
            {
                _buildsBarOpened = value;
                OnPropertyChanged("BuildsBarOpened");
            }
        }

        private bool _treeComparisonEnabled;
        public bool TreeComparisonEnabled
        {
            get { return _treeComparisonEnabled; }
            set
            {
                _treeComparisonEnabled = value;
                OnPropertyChanged("TreeComparisonEnabled");
            }
        }

        private int _selectedBuildIndex = -1;
        public int SelectedBuildIndex
        {
            get { return _selectedBuildIndex; }
            set 
            { 
                _selectedBuildIndex = value;
                OnPropertyChanged("SelectedBuildIndex");
            }
        }

        public Options()
        {
            // Don't set Language property! When not set, L10n.Initialize will try to use OS settings.
            Theme = "Dark";
            Accent = "Steel";
        }

        private void OnPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
