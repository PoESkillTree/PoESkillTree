using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using POESKillTree.Common.ViewModels;
using POESKillTree.Localization;
using POESKillTree.Model.Builds;

namespace POESKillTree.ViewModels
{
    public class DownloadItemsViewModel : CloseableViewModel
    {
        private PoEBuild _build;
        public PoEBuild Build
        {
            get { return _build; }
            private set { SetProperty(ref _build, value); }
        }

        private string _itemsLink;
        public string ItemsLink
        {
            get { return _itemsLink; }
            private set { SetProperty(ref _itemsLink, value);}
        }

        private RelayCommand _openInBrowserCommand;
        public ICommand OpenInBrowserCommand
        {
            get
            {
                return _openInBrowserCommand ??
                       (_openInBrowserCommand = new RelayCommand(() => Process.Start(ItemsLink)));
            }
        }

        public DownloadItemsViewModel(PoEBuild build)
        {
            DisplayName = L10n.Message("Download & Import Items");
            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            BuildOnPropertyChanged(this, null);
            RequestsClose += _ => Build.PropertyChanged -= BuildOnPropertyChanged;
        }

        private void BuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ItemsLink =
                string.Format("https://www.pathofexile.com/character-window/get-items?character={0}&accountName={1}",
                    Build.CharacterName, Build.AccountName);
            // Jewel data: http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character={0}&accountName={1}
        }
    }
}