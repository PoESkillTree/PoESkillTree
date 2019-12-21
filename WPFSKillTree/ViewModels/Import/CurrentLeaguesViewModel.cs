using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NLog;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Localization;
using PoESkillTree.Model.Builds;
using PoESkillTree.Utils;

namespace PoESkillTree.ViewModels.Import
{
    public class CurrentLeaguesViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string LeaguesEndpoint = "https://www.pathofexile.com/api/leagues?type=main&compat=1&";

        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<Realm, NotifyingTask<IReadOnlyList<string>>> _leagueTasks =
            new Dictionary<Realm, NotifyingTask<IReadOnlyList<string>>>();

        public CurrentLeaguesViewModel(IDialogCoordinator dialogCoordinator, HttpClient httpClient)
        {
            _dialogCoordinator = dialogCoordinator;
            _httpClient = httpClient;
        }

        public NotifyingTask<IReadOnlyList<string>> this[Realm realm] =>
            _leagueTasks.GetOrAdd(realm, CreateTask);

        private NotifyingTask<IReadOnlyList<string>> CreateTask(Realm realm) =>
            new NotifyingTask<IReadOnlyList<string>>(LoadAsync(realm),
                async e =>
                {
                    Log.Error(e, "Could not load the currently running leagues for realm " + realm);
                    await _dialogCoordinator.ShowWarningAsync(this,
                        L10n.Message("Could not load the currently running leagues."), e.Message);
                });

        private async Task<IReadOnlyList<string>> LoadAsync(Realm realm)
        {
            var file = await _httpClient.GetStringAsync($"{LeaguesEndpoint}realm={realm.ToGGGIdentifier()}")
                .ConfigureAwait(false);
            return JArray.Parse(file).Select(t => t["id"]!.Value<string>()).ToList();
        }
    }
}