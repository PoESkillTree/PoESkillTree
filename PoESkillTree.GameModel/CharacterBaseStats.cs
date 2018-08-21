using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnumsNET;
using Newtonsoft.Json;

namespace PoESkillTree.GameModel
{
    public class CharacterBaseStats
    {
        private readonly IReadOnlyDictionary<CharacterClass, JsonCharacterBaseStats> _charactertDict;

        public CharacterBaseStats(IReadOnlyDictionary<CharacterClass, JsonCharacterBaseStats> charactertDict)
        {
            _charactertDict = charactertDict;
        }

        public static async Task<CharacterBaseStats> CreateAsync()
        {
            var characters = await DataUtils.LoadRePoEAsync<JsonCharacter[]>("characters").ConfigureAwait(false);
            var characterDict = characters.ToDictionary(c => Enums.Parse<CharacterClass>(c.Name), c => c.BaseStats);
            return new CharacterBaseStats(characterDict);
        }

        public int Life(CharacterClass c) => _charactertDict[c].Life;

        public int Mana(CharacterClass c) => _charactertDict[c].Mana;

        public int Strength(CharacterClass c) => _charactertDict[c].Strength;

        public int Dexterity(CharacterClass c) => _charactertDict[c].Dexterity;

        public int Intelligence(CharacterClass c) => _charactertDict[c].Intelligence;

        public int UnarmedAttackTime(CharacterClass c) => _charactertDict[c].Unarmed.AttackTime;

        public int UnarmedPhysicalDamageMinimum(CharacterClass c) => _charactertDict[c].Unarmed.PhysicalDamageMinimum;

        public int UnarmedPhysicalDamageMaximum(CharacterClass c) => _charactertDict[c].Unarmed.PhysicalDamageMaximum;

        public int UnarmedRange(CharacterClass c) => _charactertDict[c].Unarmed.Range;
    }

    public class JsonCharacter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("base_stats")]
        public JsonCharacterBaseStats BaseStats { get; set; }
    }

    public class JsonCharacterBaseStats
    {
        [JsonProperty("life")]
        public int Life { get; set; }

        [JsonProperty("mana")]
        public int Mana { get; set; }

        [JsonProperty("strength")]
        public int Strength { get; set; }

        [JsonProperty("dexterity")]
        public int Dexterity { get; set; }

        [JsonProperty("intelligence")]
        public int Intelligence { get; set; }

        [JsonProperty("unarmed")]
        public JsonCharacterUnarmedBaseStats Unarmed { get; set; }
    }

    public class JsonCharacterUnarmedBaseStats
    {
        [JsonProperty("attack_time")]
        public int AttackTime { get; set; }

        [JsonProperty("min_physical_damage")]
        public int PhysicalDamageMinimum { get; set; }

        [JsonProperty("max_physical_damage")]
        public int PhysicalDamageMaximum { get; set; }

        [JsonProperty("range")]
        public int Range { get; set; }
    }
}