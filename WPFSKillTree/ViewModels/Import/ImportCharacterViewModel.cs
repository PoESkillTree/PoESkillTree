using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NLog;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Import
{
    public class ImportCharacterViewModel : CloseableViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string ItemsEndpoint = "https://www.pathofexile.com/character-window/get-items?";
        private const string PassiveTreeEndpoint = "https://www.pathofexile.com/character-window/get-passive-skills?reqData=0";

        private readonly HttpClient _httpClient;
        private readonly IDialogCoordinator _dialogCoordinator;
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

        private NotifyingTask<string?> _importItemsTask = NotifyingTask<string?>.FromResult(default);

        public NotifyingTask<string?> ImportItemsTask
        {
            get => _importItemsTask;
            private set => SetProperty(ref _importItemsTask, value);
        }

        private NotifyingTask<string?> _importPassiveTreeTask = NotifyingTask<string?>.FromResult(default);

        public NotifyingTask<string?> ImportPassiveTreeTask
        {
            get => _importPassiveTreeTask;
            private set => SetProperty(ref _importPassiveTreeTask, value);
        }

        private ICommand? _importItemsSkillsAndLevelCommand;
        public ICommand ImportItemsSkillsAndLevelCommand =>
            _importItemsSkillsAndLevelCommand ??= new AsyncRelayCommand(ImportItemsSkillsAndLevelAsync, CanImport);

        private ICommand? _importItemsCommand;
        public ICommand ImportItemsCommand =>
            _importItemsCommand ??= new AsyncRelayCommand(ImportItemsAsync, CanImport);

        private ICommand? _importSkillsCommand;
        public ICommand ImportSkillsCommand =>
            _importSkillsCommand ??= new AsyncRelayCommand(ImportSkillsAsync, CanImport);

        private ICommand? _importLevelCommand;
        public ICommand ImportILevelCommand =>
            _importLevelCommand ??= new AsyncRelayCommand(ImportLevelAsync, CanImport);

        private ICommand? _importPassiveTreeAndJewelsCommand;
        public ICommand ImportPassiveTreeAndJewelsCommand =>
            _importPassiveTreeAndJewelsCommand ??= new AsyncRelayCommand(ImportPassiveTreeAndJewelsAsync, CanImport);

        private ICommand? _importPassiveTreeCommand;
        public ICommand ImportPassiveTreeCommand =>
            _importPassiveTreeCommand ??= new AsyncRelayCommand(ImportPassiveTreeAsync, CanImport);

        private ICommand? _importJewelsCommand;
        public ICommand ImportJewelsCommand =>
            _importJewelsCommand ??= new AsyncRelayCommand(ImportJewelsAsync, CanImport);

        public ImportCharacterViewModel(
            HttpClient httpClient, IDialogCoordinator dialogCoordinator, PoEBuild build,
            CurrentLeaguesViewModel currentLeagues, AccountCharactersViewModel accountCharacters)
        {
            _httpClient = httpClient;
            _dialogCoordinator = dialogCoordinator;
            _currentLeaguesViewModel = currentLeagues;
            _accountCharactersViewModel = accountCharacters;
            DisplayName = L10n.Message("Import Character");
            Build = build;
            Build.PropertyChanged += BuildOnPropertyChanged;
            _currentLeagues = _currentLeaguesViewModel[Build.Realm];
            _accountCharacters = GetAccountCharacters();
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
        }

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> GetAccountCharacters()
        {
            var task = _accountCharactersViewModel.Get(Build.Realm, Build.AccountName).Select(cs =>
                cs.Where(c => string.IsNullOrEmpty(Build.League) || c.League == Build.League)
                    .OrderBy(c => c.Name)
                    .ToList());
            return task;
        }

        private bool CanImport() =>
            !string.IsNullOrEmpty(Build.CharacterName) && (PrivateProfile || !string.IsNullOrEmpty(Build.AccountName));

        // TODO import functionality
        private async Task ImportItemsSkillsAndLevelAsync()
        {
            var importJson = await RequestItemsAsync(L10n.Message("Import Items, Skills and Level"));
            if (importJson is null)
                return;
        }

        private async Task ImportItemsAsync()
        {
            var importJson = await RequestItemsAsync(L10n.Message("Import Items"));
            if (importJson is null)
                return;
        }

        private async Task ImportSkillsAsync()
        {
            var importJson = await RequestItemsAsync(L10n.Message("Import Skills"));
            if (importJson is null)
                return;
        }

        private async Task ImportLevelAsync()
        {
            var importJson = await RequestItemsAsync(L10n.Message("Import Level"));
            if (importJson is null)
                return;
        }

        private async Task ImportPassiveTreeAndJewelsAsync()
        {
            var importJson = await RequestPassiveTreeAsync(L10n.Message("Import Passive Tree and Jewels"));
            if (importJson is null)
                return;
        }

        private async Task ImportPassiveTreeAsync()
        {
            var importJson = await RequestPassiveTreeAsync(L10n.Message("Import Passive Tree"));
            if (importJson is null)
                return;
        }

        private async Task ImportJewelsAsync()
        {
            var importJson = await RequestPassiveTreeAsync(L10n.Message("Import Jewels"));
            if (importJson is null)
                return;
        }

        private async Task<string?> RequestItemsAsync(string title)
        {
            ImportItemsTask = new NotifyingTask<string?>(RequestAsync(ItemsUrl, title),
                e => Log.Error($"Could not retrieve {ItemsUrl}"));
            await ImportItemsTask.TaskCompletion;
            return ImportItemsTask.IsSuccessfullyCompleted ? ImportItemsTask.Result : null;
        }

        private async Task<string?> RequestPassiveTreeAsync(string title)
        {
            ImportPassiveTreeTask = new NotifyingTask<string?>(RequestAsync(PassiveTreeUrl, title),
                e => Log.Error($"Could not retrieve {PassiveTreeUrl}"));
            await ImportPassiveTreeTask.TaskCompletion;
            return ImportPassiveTreeTask.IsSuccessfullyCompleted ? ImportPassiveTreeTask.Result : null;
        }

        private async Task<string?> RequestAsync(string url, string title)
        {
            if (PrivateProfile)
            {
                var message = L10n.Message("A tab in your default web browser has been opened containing your character's data.") + "\n"
                    + L10n.Message("Copy its contents and paste them into the field below.") + "\n"
                    + L10n.Message("In case no browser has been opened, the URL was also copied to your clipboard.");
                var task = _dialogCoordinator.ShowInputAsync(this, title, message);
                Clipboard.SetText(url);
                await Task.Delay(TimeSpan.FromMilliseconds(200));
                Util.OpenInBrowser(url);
                return await task;
            }
            else
            {
                var result = await _httpClient.GetAsync(url);
                result.EnsureSuccessStatusCode();
                return await result.Content.ReadAsStringAsync();
            }
        }

        private string ItemsUrl => ItemsEndpoint + GetQueryString();
        private string PassiveTreeUrl => PassiveTreeEndpoint + GetQueryString();

        private string GetQueryString()
        {
            var query = $"realm={Build.Realm.ToGGGIdentifier()}&character={Build.CharacterName}";
            if (!string.IsNullOrEmpty(Build.AccountName))
                query += $"&accountName={Build.AccountName}";
            return query;
        }
    }
}