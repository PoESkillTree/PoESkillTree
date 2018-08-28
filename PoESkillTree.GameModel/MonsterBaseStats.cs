using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PoESkillTree.GameModel
{
    public class MonsterBaseStats
    {
        private readonly IReadOnlyDictionary<int, JsonMonsterBaseStatsForLevel> _statDict;

        public MonsterBaseStats(IReadOnlyDictionary<int, JsonMonsterBaseStatsForLevel> statDict)
        {
            _statDict = statDict;
        }

        public static async Task<MonsterBaseStats> CreateAsync()
        {
            var statDict =
                await DataUtils.LoadRePoEAsync<IReadOnlyDictionary<int, JsonMonsterBaseStatsForLevel>>(
                    "default_monster_stats").ConfigureAwait(false);
            return new MonsterBaseStats(statDict);
        }

        public double PhysicalDamage(int level) => _statDict[level].PhysicalDamage;

        public int Accuracy(int level) => _statDict[level].Accuracy;

        public int Evasion(int level) => _statDict[level].Evasion;

        public int EnemyLife(int level) => _statDict[level].EnemyLife;

        public int AllyLife(int level) => _statDict[level].AllyLife;
    }

    public class JsonMonsterBaseStatsForLevel
    {
        [JsonProperty("physical_damage")]
        public double PhysicalDamage { get; set; }

        [JsonProperty("accuracy")]
        public int Accuracy { get; set; }
        
        [JsonProperty("evasion")]
        public int Evasion { get; set; }
        
        [JsonProperty("life")]
        public int EnemyLife { get; set; }
        
        [JsonProperty("ally_life")]
        public int AllyLife { get; set; }
    }
}