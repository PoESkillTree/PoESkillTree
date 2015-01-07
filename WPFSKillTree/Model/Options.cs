using System.ComponentModel;
namespace POESKillTree.Model
{
    public class Options : INotifyPropertyChanged
    {
        public string Theme { get; set; }
        public string Accent { get; set; } //Controlled by Menu Accent Headers
        public bool AttributesBarOpened { get; set; }
        public bool BuildsBarOpened { get; set; }


        private bool _TreeComparisonEnabled = false;
        public bool TreeComparisonEnabled
        {
            get { return _TreeComparisonEnabled; }
            set
            {
                _TreeComparisonEnabled = value;
                OnPropertyChanged("TreeComparisonEnabled");
            }
        }

        private int _SelectedBuildIndex = -1;

        public int SelectedBuildIndex
        {
            get { return _SelectedBuildIndex; }
            set 
            { 
                _SelectedBuildIndex = value;
                OnPropertyChanged("SelectedBuildIndex");
            }
        }

        public Options()
        {
            Theme = "Dark";
            Accent = "Steel";
            AttributesBarOpened = false;
            BuildsBarOpened = false;
        }

        private void OnPropertyChanged(string caller)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
