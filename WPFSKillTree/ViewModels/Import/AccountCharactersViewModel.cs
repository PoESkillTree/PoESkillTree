using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Builds;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Import
{
    public class AccountCharactersViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string CharactersEndpoint = "https://www.pathofexile.com/character-window/get-characters?";

        private readonly HttpClient _httpClient;
        private readonly Dictionary<(Realm, string?), NotifyingTask<IReadOnlyList<AccountCharacterViewModel>>> _tasks =
            new Dictionary<(Realm, string?), NotifyingTask<IReadOnlyList<AccountCharacterViewModel>>>();

        public AccountCharactersViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> Get(Realm realm, string? accountName) =>
            _tasks.GetOrAdd((realm, accountName), CreateTask);

        private NotifyingTask<IReadOnlyList<AccountCharacterViewModel>> CreateTask((Realm realm, string? accountName) tuple) =>
            new NotifyingTask<IReadOnlyList<AccountCharacterViewModel>>(LoadAsync(tuple.realm, tuple.accountName),
                Array.Empty<AccountCharacterViewModel>(),
                e => Log.Error(e, $"Could not retrieve the characters of {tuple.accountName} on {tuple.realm}"));

        private async Task<IReadOnlyList<AccountCharacterViewModel>> LoadAsync(Realm realm, string? accountName)
        {
            if (string.IsNullOrEmpty(accountName))
                return Array.Empty<AccountCharacterViewModel>();

            var requestUrl = $"{CharactersEndpoint}realm={realm.ToGGGIdentifier()}&accountName={accountName}";
            var result = await _httpClient.GetAsync(requestUrl);
            result.EnsureSuccessStatusCode();
            var contents = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IReadOnlyList<AccountCharacterViewModel>>(contents);
        }
    }
}