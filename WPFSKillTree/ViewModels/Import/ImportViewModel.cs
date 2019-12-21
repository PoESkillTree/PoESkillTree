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

        public ImportViewModels(IDialogCoordinator dialogCoordinator, IPersistentData persistentData, StashViewModel stash)
        {
            _dialogCoordinator = dialogCoordinator;
            _persistentData = persistentData;
            _stash = stash;
            var httpClient = new HttpClient();
            _currentLeagues = new Lazy<CurrentLeaguesViewModel>(() => new CurrentLeaguesViewModel(dialogCoordinator, httpClient));
        }

        public CurrentLeaguesViewModel CurrentLeagues => _currentLeagues.Value;
        public ImportCharacterViewModel ImportCharacter => new ImportCharacterViewModel(_persistentData.CurrentBuild);
        public ImportStashViewModel ImportStash => new ImportStashViewModel(_dialogCoordinator, _persistentData, _stash, CurrentLeagues);
    }
}