using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.Common.ViewModels;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;

namespace PoESkillTree.ViewModels.Equipment
{
    public class DownloadItemsViewModel : CloseableViewModel
    {
        public PoEBuild Build { get; }

        private readonly HttpClient HttpClient = new HttpClient();

        private RelayCommand? loadAccountCommand;
        public ICommand LoadAccountCommand => loadAccountCommand ??= new RelayCommand(async () =>
            {
                AccountCharacters.Clear();
                AccountCharactersLoaded = false;

                using var res = await HttpClient.GetAsync(
                    $"https://www.pathofexile.com/character-window/get-characters?accountName={Build.AccountName}&realm=pc");

                if (res.IsSuccessStatusCode)
                    try
                    {
                        foreach (var c in JsonConvert.DeserializeObject<AccountCharacter[]>(await res.Content.ReadAsStringAsync()).OrderBy(w => w.League).OrderBy(w => w.Level))
                            AccountCharacters.Add(c);
                        SelectedAccountCharacter = AccountCharacters.FirstOrDefault();
                        AccountCharactersLoaded = true;
                    }
                    catch { }
            });

        public class AccountCharacter
        {
            public string? Name { get; set; }
            public string? League { get; set; }
            public int ClassId { get; set; }
            public string? Class { get; set; }
            public int AscendancyClass { get; set; }
            public int Level { get; set; }
            public string DisplayText => $"{(League![0] == 'S' ? 'S' : 'T')}{Level:00} {Class}: {Name}";
        }
        public ObservableCollection<AccountCharacter> AccountCharacters { get; } = new ObservableCollection<AccountCharacter>();

        private AccountCharacter? selectedAccountCharacter;
        public AccountCharacter? SelectedAccountCharacter
        {
            get => selectedAccountCharacter;
            set => SetProperty(ref selectedAccountCharacter, value);
        }

        private bool accountCharactersLoaded;
        public bool AccountCharactersLoaded
        {
            get => accountCharactersLoaded;
            private set => SetProperty(ref accountCharactersLoaded, value);
        }

        private RelayCommand? loadCharacterCommand;
        public ICommand LoadCharacterCommand => loadCharacterCommand ??= new RelayCommand(async () =>
            {
                try
                {
                    // load the items in the build
                    Build.ItemData = await HttpClient.GetStringAsync(
                        $"https://www.pathofexile.com/character-window/get-items?character={SelectedAccountCharacter!.Name}&accountName={Build.AccountName}");

                    // load the passives in the build, by transforming the hashes json array to a poe passive skill tree url
                    var hashesjsonobj = JObject.Parse(await HttpClient.GetStringAsync(
                        $"http://www.pathofexile.com/character-window/get-passive-skills?reqData=0&character={SelectedAccountCharacter!.Name}&accountName={Build.AccountName}"));
                    var hashes = ((JArray)hashesjsonobj["hashes"]!).Select(w => w.Value<int>()).ToList();

                    var buffer = new byte[7 + hashes.Count * 2];
                    buffer[3] = 4;
                    buffer[4] = (byte)SelectedAccountCharacter.ClassId;
                    buffer[5] = (byte)SelectedAccountCharacter.AscendancyClass;

                    for (int idx = 0; idx < hashes.Count; ++idx)
                    {
                        buffer[7 + idx * 2] = (byte)(hashes[idx] >> 8 & 0xFF);
                        buffer[8 + idx * 2] = (byte)(hashes[idx] & 0xFF);
                    }

                    Build.TreeUrl = "https://www.pathofexile.com/passive-skill-tree/3.9/" + Convert.ToBase64String(buffer).Replace('+', '-').Replace('/', '_');
                }
                catch { }
            });

        public DownloadItemsViewModel(PoEBuild build)
        {
            DisplayName = L10n.Message("Download & Import Items");
            Build = build;
        }

        protected override void OnClose()
        {
        }
    }
}