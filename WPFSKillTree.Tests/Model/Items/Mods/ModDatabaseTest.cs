using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Modifiers;

namespace PoESkillTree.Model.Items.Mods
{
    [TestFixture]
    public class ModDatabaseTest
    {
        private static readonly ISet<string> UnknownTags = new HashSet<string>
        {
            // only added by other mods, which is not supported anyway
            "no_attack_mods", "no_caster_mods", "has_physical_conversion_mod", "has_damage_taken_as_mod",
            "specific_weapon", "two_handed_mod", "shield_mod", "dual_wielding_mod", "one_handed_mod", "melee_mod",
            "grants_crit_chance_support", "local_item_quality", "spell_dodge_mod", "weapon_can_roll_minion_modifiers",
            "grants_2h_support",
            // map crafting is not supported
            "map", "unique_map", "no_monster_packs",
            // no idea where these come from
            "no_elemental_damage_mods", "no_physical_damage_mods",
        };
        private static readonly ISet<string> UnknownItemClasses = new HashSet<string>
        {
            // map crafting is not supported
            "Map", "MapFragment",
        };

#pragma warning disable 8618 // Initialized in InitializeAsync
        private static Dictionary<string, JsonMod> _mods;
        private static JsonCraftingBenchOption[] _benchOptions;
        private static ModDatabase _modDatabase;
#pragma warning restore

        [OneTimeSetUp]
        public static async Task InitializeAsync()
        {
            _mods = await DataUtils.LoadRePoEAsync<Dictionary<string, JsonMod>>("mods");
            _benchOptions = await DataUtils.LoadRePoEAsync<JsonCraftingBenchOption[]>("crafting_bench_options");
            _modDatabase = new ModDatabase(_mods, _benchOptions);
        }

        [Test]
        public void JsonMod_AccuracyAndCritsJewel()
        {
            var mod = _mods["AccuracyAndCritsJewel"];

            Assert.AreEqual(ModDomain.Misc, mod.Domain);
            Assert.AreEqual(ModGenerationType.Suffix, mod.GenerationType);
            Assert.AreEqual("AccuracyAndCrits", mod.Group);
            Assert.AreEqual(false, mod.IsEssenceOnly);
            Assert.AreEqual("of Deadliness", mod.Name);
            Assert.AreEqual(1, mod.RequiredLevel);
            Assert.AreEqual(2, mod.SpawnWeights.Length);
            Assert.AreEqual(2, mod.Stats.Length);

            var spawnWeight = mod.SpawnWeights[0];
            Assert.AreEqual("not_dex", spawnWeight.Tag);
            Assert.AreEqual(false, spawnWeight.CanSpawn);
            spawnWeight = mod.SpawnWeights[1];
            Assert.AreEqual("default", spawnWeight.Tag);
            Assert.AreEqual(true, spawnWeight.CanSpawn);

            var stat = mod.Stats[0];
            Assert.AreEqual("accuracy_rating_+%", stat.Id);
            Assert.AreEqual(10, stat.Max);
            Assert.AreEqual(6, stat.Min);
            stat = mod.Stats[1];
            Assert.AreEqual("critical_strike_chance_+%", stat.Id);
            Assert.AreEqual(10, stat.Max);
            Assert.AreEqual(6, stat.Min);
        }

        [Test]
        public void JsonMod_LocalIncreasedAccuracy6()
        {
            var mod = _mods["LocalIncreasedAccuracyNew3"];

            Assert.AreEqual(ModDomain.Item, mod.Domain);
            Assert.AreEqual(ModGenerationType.Suffix, mod.GenerationType);
            Assert.AreEqual("IncreasedAccuracy", mod.Group);
            Assert.AreEqual(false, mod.IsEssenceOnly);
            Assert.AreEqual("of the Sniper", mod.Name);
            Assert.AreEqual(40, mod.RequiredLevel);
            Assert.AreEqual(3, mod.SpawnWeights.Length);
            Assert.AreEqual(1, mod.Stats.Length);

            var spawnWeight = mod.SpawnWeights[0];
            Assert.AreEqual("no_attack_mods", spawnWeight.Tag);
            Assert.AreEqual(false, spawnWeight.CanSpawn);
            spawnWeight = mod.SpawnWeights[1];
            Assert.AreEqual("weapon", spawnWeight.Tag);
            Assert.AreEqual(true, spawnWeight.CanSpawn);
            spawnWeight = mod.SpawnWeights[2];
            Assert.AreEqual("default", spawnWeight.Tag);
            Assert.AreEqual(false, spawnWeight.CanSpawn);

            var stat = mod.Stats[0];
            Assert.AreEqual("local_accuracy_rating", stat.Id);
            Assert.AreEqual(325, stat.Max);
            Assert.AreEqual(216, stat.Min);
        }

        // make sure essence mods can't spawn through tags so they need to be handled differently
        [Test]
        public void EssenceMods_NoSpawnTags()
        {
            foreach (var mod in _mods.Values)
            {
                if (mod.IsEssenceOnly)
                {
                    AssertCantSpawn(mod);
                }
            }
        }

        private void AssertCantSpawn(JsonMod mod)
        {
            foreach (var spawnWeight in mod.SpawnWeights)
            {
                Assert.AreEqual("default", spawnWeight.Tag);
                Assert.IsFalse(spawnWeight.CanSpawn);
            }
        }

        [Test]
        public void GetMatchingMods_ChaosDamage()
        {
            var affixes = _modDatabase[ModGenerationType.Prefix];

            var chaosDamage = affixes.Single(a => a.Group == "ChaosDamage");
            Assert.AreEqual(4, chaosDamage.Mods.Count);
            Assert.AreEqual("ChaosDamageJewel", ((Mod) chaosDamage.Mods[0]).Id);
            Assert.AreEqual("EinharMasterAddedChaosDamage1", ((Mod) chaosDamage.Mods[1]).Id);

            var bowChaosDamage = chaosDamage.GetMatchingMods(
                Tags.Bow | Tags.TwoHandWeapon | Tags.Ranged, ItemClass.Bow).ToList();
            Assert.AreEqual(0, bowChaosDamage.Count);
            var jewelChaosDamage = chaosDamage.GetMatchingMods(
                Tags.Jewel | Tags.DexJewel | Tags.NotInt | Tags.NotStr, ItemClass.Jewel).ToList();
            Assert.AreEqual(1, jewelChaosDamage.Count);
            Assert.AreEqual("ChaosDamageJewel", ((Mod) jewelChaosDamage[0]).Id);
            var amuletChaosDamage = chaosDamage.GetMatchingMods(Tags.Amulet, ItemClass.Amulet).ToList();
            Assert.AreEqual(2, amuletChaosDamage.Count);
            Assert.AreEqual("EinharMasterAddedChaosDamage1", ((Mod) amuletChaosDamage[0]).Id);
        }

        [Test]
        public void GetMatchingMods_ProjectileSpeed()
        {
            var affixes = _modDatabase[ModGenerationType.Suffix];
            var affix = affixes.Single(a => a.Group == "ProjectileSpeed");

            var quiver = affix.GetMatchingMods(Tags.Quiver, ItemClass.Quiver).ToList();
            Assert.AreEqual(5, quiver.Count);
            Assert.AreEqual("ProjectileSpeed1", ((Mod) quiver[0]).Id);
        }

        [Test]
        public void GetMatchingMods_DefencesPercent_NoMasterMods()
        {
            var affixes = _modDatabase[ModGenerationType.Prefix];
            var affix = affixes.Single(a => a.Group == "DefencesPercent");

            var dexHelmet = affix.GetMatchingMods(Tags.Armour | Tags.Helmet | Tags.DexArmour, ItemClass.Helmet)
                .ToList();
            Assert.IsTrue(dexHelmet.Any());
            Assert.IsFalse(dexHelmet.Any(m => m.Domain == ModDomain.Crafted));
        }

        [Test]
        public void GetMatchingMods_IncreasedLife_MasterMods()
        {
            var affixes = _modDatabase[ModGenerationType.Prefix];
            var affix = affixes.Single(a => a.Group == "IncreasedLife");

            var dexHelmet = affix.GetMatchingMods(Tags.Armour | Tags.Helmet | Tags.DexArmour, ItemClass.Helmet)
                .ToList();
            Assert.IsTrue(dexHelmet.Any(m => m.Domain == ModDomain.Crafted));
        }

        // Make sure all possible Tags and ItemClasses are either known or purposefully unknown.

        [Test]
        public void JsonMod_UnknownTags()
        {
            var unexpectedTags = (
                from mod in _mods.Values
                where mod.Domain != ModDomain.Area && mod.Domain != ModDomain.Atlas
                from spawnWeight in mod.SpawnWeights
                let tag = spawnWeight.Tag
                where !tag.EndsWith("_shaper") && !tag.EndsWith("_elder")
                      && !tag.EndsWith("_crusader") && !tag.EndsWith("_eyrie") && !tag.EndsWith("_basilisk") && !tag.EndsWith("_adjudicator")
                      && !TagsExtensions.TryParse(tag, out _)
                      && !UnknownTags.Contains(tag)
                select tag
            ).ToHashSet();
            Assert.AreEqual(0, unexpectedTags.Count, string.Join(", ", unexpectedTags));
        }

        [Test]
        public void JsonCraftingBenchOption_UnknownItemClasses()
        {
            foreach (var benchOption in _benchOptions)
            {
                foreach (var itemClass in benchOption.ItemClasses)
                {
                    if (!ItemClassEx.TryParse(itemClass, out _))
                    {
                        Assert.IsTrue(UnknownItemClasses.Contains(itemClass), itemClass + " unknown");
                    }
                }
            }
        }
    }
}