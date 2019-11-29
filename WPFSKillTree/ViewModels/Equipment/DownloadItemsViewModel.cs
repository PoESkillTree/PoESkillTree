using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Equipment
{
    public class DownloadItemsViewModel : CloseableViewModel
    {
        public PoEBuild Build { get; }

        private string _itemsLink;
        public string ItemsLink
        {
            get => _itemsLink;
            private set => SetProperty(ref _itemsLink, value);
        }

        private RelayCommand? _openInBrowserCommand;
        public ICommand OpenInBrowserCommand => _openInBrowserCommand ??= new RelayCommand(() => Process.Start(ItemsLink));

        public DownloadItemsViewModel(PoEBuild build)
        {
            DisplayName = L10n.Message("Download & Import Items");
            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            _itemsLink = CreateItemsLink();
        }

        protected override void OnClose()
        {
            Build.PropertyChanged -= BuildOnPropertyChanged;
        }

        private void BuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ItemsLink = CreateItemsLink();
            // Jewel data: http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character={0}&accountName={1}
        }

        private string CreateItemsLink() =>
            $"https://www.pathofexile.com/character-window/get-items?character={Build.CharacterName}&accountName={Build.AccountName}";
    }
}