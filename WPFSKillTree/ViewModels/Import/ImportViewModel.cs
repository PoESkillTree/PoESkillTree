using System;
using System.Net.Http;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Model;
using PoESkillTree.ViewModels.Equipment;

namespace PoESkillTree.ViewModels.Import
{
    public class ImportViewModels
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IPersistentData _persistentData;
        private readonly StashViewModel _stash;
        private readonly Lazy<CurrentLeaguesViewModel> _currentLeagues;
        private readonly Lazy<AccountCharactersViewModel> _accountCharacters;

        public ImportViewModels(IDialogCoordinator dialogCoordinator, IPersistentData persistentData, StashViewModel stash)
        {
            _dialogCoordinator = dialogCoordinator;
            _persistentData = persistentData;
            _stash = stash;
            var httpClient = new HttpClient();
            _currentLeagues = new Lazy<CurrentLeaguesViewModel>(() => new CurrentLeaguesViewModel(httpClient));
            _accountCharacters = new Lazy<AccountCharactersViewModel>(() => new AccountCharactersViewModel(httpClient));
        }

        public ImportCharacterViewModel ImportCharacter =>
            new ImportCharacterViewModel(_persistentData.CurrentBuild, _currentLeagues.Value, _accountCharacters.Value);

        public ImportStashViewModel ImportStash => new ImportStashViewModel(_dialogCoordinator, _persistentData, _stash, _currentLeagues.Value);
    }
}