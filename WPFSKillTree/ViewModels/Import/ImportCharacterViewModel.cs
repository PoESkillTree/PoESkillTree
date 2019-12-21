using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Import
{
    public class ImportCharacterViewModel : CloseableViewModel
    {
        private readonly CurrentLeaguesViewModel _currentLeaguesViewModel;
        private readonly AccountCharactersViewModel _accountCharactersViewModel;

        public PoEBuild Build { get; }

        private bool _privateProfile;

        public bool PrivateProfile
        {
            get => _privateProfile;
            set => SetProperty(ref _privateProfile, value, () => OnPropertyChanged(nameof(PublicProfile)));
        }

        public bool PublicProfile => !PrivateProfile;

        private NotifyingTask<IReadOnlyList<string>> _currentLeagues;

        public NotifyingTask<IReadOnlyList<string>> CurrentLeagues
        {
            get => _currentLeagues;
            private set => SetProperty(ref _currentLeagues, value);
        }

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> _accountCharacters;

        public NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> AccountCharacters
        {
            get => _accountCharacters;
            private set => SetProperty(ref _accountCharacters, value);
        }

        private string _itemsLink;
        public string ItemsLink
        {
            get => _itemsLink;
            private set => SetProperty(ref _itemsLink, value);
        }

        private RelayCommand? _openInBrowserCommand;
        public ICommand OpenInBrowserCommand => _openInBrowserCommand ??= new RelayCommand(() => Util.OpenInBrowser(ItemsLink));

        public ImportCharacterViewModel(PoEBuild build, CurrentLeaguesViewModel currentLeagues, AccountCharactersViewModel accountCharacters)
        {
            _currentLeaguesViewModel = currentLeagues;
            _accountCharactersViewModel = accountCharacters;
            DisplayName = L10n.Message("Import Character");
            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            _currentLeagues = _currentLeaguesViewModel[Build.Realm];
            _accountCharacters = GetAccountCharacters();
            _itemsLink = CreateItemsLink();
        }

        protected override void OnClose()
        {
            Build.PropertyChanged -= BuildOnPropertyChanged;
        }

        private void BuildOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var propertyName = propertyChangedEventArgs.PropertyName;
            if (propertyName == nameof(PoEBuild.Realm))
            {
                CurrentLeagues = _currentLeaguesViewModel[Build.Realm];
            }

            if (propertyName == nameof(PoEBuild.Realm) || propertyName == nameof(PoEBuild.AccountName) || propertyName == nameof(PoEBuild.League))
            {
                AccountCharacters = GetAccountCharacters();
            }

            ItemsLink = CreateItemsLink();
            // Jewel data: http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character={0}&accountName={1}
        }

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> GetAccountCharacters()
        {
            var task = _accountCharactersViewModel.Get(Build.Realm, Build.AccountName).Select(cs =>
                cs.Where(c => string.IsNullOrEmpty(Build.League) || c.League == Build.League)
                    .OrderBy(c => c.Name)
                    .ToList());
            return task;
        }

        private string CreateItemsLink()
        {
            var link =
                $"https://www.pathofexile.com/character-window/get-items?realm={Build.Realm.ToGGGIdentifier()}&character={Build.CharacterName}";
            if (!string.IsNullOrEmpty(Build.AccountName))
                link += $"&accountName={Build.AccountName}";
            return link;
        }
    }
}