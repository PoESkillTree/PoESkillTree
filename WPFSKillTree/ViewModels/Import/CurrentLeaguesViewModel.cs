using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Builds;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Import
{
    public class CurrentLeaguesViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string LeaguesEndpoint = "https://www.pathofexile.com/api/leagues?type=main&compat=1&";

        private readonly HttpClient _httpClient;
        private readonly Dictionary<Realm, NotifyingTask<IReadOnlyList<string>>> _leagueTasks =
            new Dictionary<Realm, NotifyingTask<IReadOnlyList<string>>>();

        public CurrentLeaguesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public NotifyingTask<IReadOnlyList<string>> this[Realm realm] =>
            _leagueTasks.GetOrAdd(realm, CreateTask);

        private NotifyingTask<IReadOnlyList<string>> CreateTask(Realm realm) =>
            new NotifyingTask<IReadOnlyList<string>>(LoadAsync(realm),
                e => Log.Error(e, "Could not retrieve the currently running leagues for realm " + realm));

        private async Task<IReadOnlyList<string>> LoadAsync(Realm realm)
        {
            var result = await _httpClient.GetAsync($"{LeaguesEndpoint}realm={realm.ToGGGIdentifier()}");
            result.EnsureSuccessStatusCode();
            var contents = await result.Content.ReadAsStringAsync();
            return JArray.Parse(contents).Select(t => t["id"]!.Value<string>()).ToList();
        }
    }
}