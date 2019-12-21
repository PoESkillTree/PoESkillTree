using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
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
        private readonly ItemAttributes _itemAttributes;

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

        private NotifyingTask<Unit> _importItemsSkillsAndLevelTask = NotifyingTask<Unit>.FromResult(default);

        public NotifyingTask<Unit> ImportItemsSkillsAndLevelTask
        {
            get => _importItemsSkillsAndLevelTask;
            private set => SetProperty(ref _importItemsSkillsAndLevelTask, value);
        }

        private NotifyingTask<Unit> _importPassiveTreeAndJewelsTask = NotifyingTask<Unit>.FromResult(default);

        public NotifyingTask<Unit> ImportPassiveTreeAndJewelsTask
        {
            get => _importPassiveTreeAndJewelsTask;
            private set => SetProperty(ref _importPassiveTreeAndJewelsTask, value);
        }

        private ICommand? _importItemsSkillsAndLevelCommand;
        public ICommand ImportItemsSkillsAndLevelCommand =>
            _importItemsSkillsAndLevelCommand ??= new RelayCommand(ImportItemsSkillsAndLevel, CanImportItemsSkillsAndLevel);

        private ICommand? _importItemsCommand;
        public ICommand ImportItemsCommand => _importItemsCommand ??= new RelayCommand(ImportItems, CanImportItemsSkillsAndLevel);

        private ICommand? _importSkillsCommand;
        public ICommand ImportSkillsCommand => _importSkillsCommand ??= new RelayCommand(ImportSkills, CanImportItemsSkillsAndLevel);

        private ICommand? _importLevelCommand;
        public ICommand ImportILevelCommand => _importLevelCommand ??= new RelayCommand(ImportLevel, CanImportItemsSkillsAndLevel);

        private ICommand? _importPassiveTreeAndJewelsCommand;
        public ICommand ImportPassiveTreeAndJewelsCommand =>
            _importPassiveTreeAndJewelsCommand ??= new RelayCommand(ImportPassiveTreeAndJewels, CanImportPassiveTreeAndJewels);

        private ICommand? _importPassiveTreeCommand;
        public ICommand ImportPassiveTreeCommand => _importPassiveTreeCommand ??= new RelayCommand(ImportPassiveTree, CanImportPassiveTreeAndJewels);

        private ICommand? _importJewelsCommand;
        public ICommand ImportJewelsCommand => _importJewelsCommand ??= new RelayCommand(ImportJewels, CanImportPassiveTreeAndJewels);

        public ImportCharacterViewModel(
            HttpClient httpClient, IDialogCoordinator dialogCoordinator,
            ItemAttributes itemAttributes,
            PoEBuild build, CurrentLeaguesViewModel currentLeagues, AccountCharactersViewModel accountCharacters)
        {
            _httpClient = httpClient;
            _dialogCoordinator = dialogCoordinator;
            _currentLeaguesViewModel = currentLeagues;
            _accountCharactersViewModel = accountCharacters;
            _itemAttributes = itemAttributes;
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

        private bool CanImportItemsSkillsAndLevel() =>
            CanImport() && ImportItemsSkillsAndLevelTask.IsCompleted;

        private bool CanImportPassiveTreeAndJewels() =>
            CanImport() && ImportPassiveTreeAndJewelsTask.IsCompleted;

        private bool CanImport() =>
            !string.IsNullOrEmpty(Build.CharacterName) && (PrivateProfile || !string.IsNullOrEmpty(Build.AccountName));

        private void ImportItemsSkillsAndLevel()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Items, Skills and Level"), true, true, true);
        }

        private void ImportItems()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Items"), items: true);
        }

        private void ImportSkills()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Skills"), skills: true);
        }

        private void ImportLevel()
        {
            StartItemSkillsAndLevelImport(L10n.Message("Import Level"), level: true);
        }

        private async void StartItemSkillsAndLevelImport(string title, bool items = false, bool skills = false, bool level = false)
        {
            ImportItemsSkillsAndLevelTask = new NotifyingTask<Unit>(ImportItemSkillsAndLevelAsync(title, items, skills, level),
                e => Log.Error($"Could not retrieve {ItemsUrl}"));
            await ImportItemsSkillsAndLevelTask.TaskCompletion;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task<Unit> ImportItemSkillsAndLevelAsync(string title, bool importItems, bool importSkills, bool importLevel)
        {
            var importJson = await RequestAsync(ItemsUrl, title);
            if (string.IsNullOrEmpty(importJson))
                return Unit.Default;

            if (importItems)
            {
                var toRemove = _itemAttributes.Equip.Where(i => i.Slot != ItemSlot.SkillTree).ToList();
                foreach (var item in toRemove)
                {
                    _itemAttributes.RemoveItem(item);
                }
            }
            if (importSkills)
            {
                var toRemove = _itemAttributes.Skills.Where(ss => ss.First().ItemSlot != ItemSlot.Unequipable).ToList();
                foreach (var skills in toRemove)
                {
                    _itemAttributes.RemoveSkills(skills);
                }
            }

            var import = JObject.Parse(importJson);
            if (importItems || importSkills)
            {
                _itemAttributes.DeserializeItemsWithSkills(import, importItems, importSkills);
            }
            if (importLevel && import.TryGetValue("character", out var characterToken))
            {
                Build.Level = characterToken.Value<int>("level");
            }
            return Unit.Default;
        }

        private void ImportPassiveTreeAndJewels()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Passive Tree and Jewels"), true, true);
        }

        private void ImportPassiveTree()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Passive Tree"), passiveTree: true);
        }

        private void ImportJewels()
        {
            StartPassiveTreeAndJewelsImport(L10n.Message("Import Jewels"), jewels: true);
        }

        private async void StartPassiveTreeAndJewelsImport(string title, bool passiveTree = false, bool jewels = false)
        {
            ImportPassiveTreeAndJewelsTask = new NotifyingTask<Unit>(ImportPassiveTreeAndJewelsAsync(title, passiveTree, jewels),
                e => Log.Error($"Could not retrieve {PassiveTreeUrl}"));
            await ImportPassiveTreeAndJewelsTask.TaskCompletion;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task<Unit> ImportPassiveTreeAndJewelsAsync(string title, bool importPassiveTree, bool importJewels)
        {
            var importJson = await RequestAsync(PassiveTreeUrl, title);
            if (string.IsNullOrEmpty(importJson))
                return Unit.Default;

            // TODO
            return Unit.Default;
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