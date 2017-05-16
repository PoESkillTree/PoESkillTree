using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;

namespace UnitTests.Model.Items.Mods
{
    [TestClass]
    public class ModDatabaseTest
    {
        private static readonly ISet<string> UnknownTags = new HashSet<string>
        {
            // only added by other mods, which is not supported anyway
            // - master mods "Cannot roll Attack Mods" and "Cannot roll Caster Mods"
            "no_attack_mods", "no_caster_mods",
            // - jewel mods
            "specific_weapon", "two_handed_mod", "shield_mod", "dual_wielding_mod", "one_handed_mod", "melee_mod",
            // map crafting is not supported
            "map",
        };
        private static readonly ISet<string> UnknownItemClasses = new HashSet<string>
        {
            // map crafting is not supported
            "Map", "MapFragment",
        };

        private Task _initialization;

        private Dictionary<string, JsonMod> _mods;
        private JsonCraftingBenchOption[] _benchOptions;
        private Dictionary<string, JsonNpcMaster> _npcMasters;
        private ModDatabase _modDatabase;

        [ClassInitialize]
        public static void StaticInitialize(TestContext testContext)
        {
            // Remove all files before running the first test to make sure they are up to date
            var dataPath = AppData.GetFolder(Path.Combine("Data", "RePoE"));
            Directory.Delete(dataPath, true);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _initialization = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var loader = new RePoELoader(new HttpClient(), false);
            _mods = await loader.LoadAsync<Dictionary<string, JsonMod>>("mods");
            _benchOptions = await loader.LoadAsync<JsonCraftingBenchOption[]>("crafting_bench_options");
            _npcMasters = await loader.LoadAsync<Dictionary<string, JsonNpcMaster>>("npc_master");
            _modDatabase = new ModDatabase(_mods, _benchOptions, _npcMasters);
        }

        [TestMethod]
        public async Task JsonMod_AccuracyAndCritsJewel()
        {
            await _initialization;
            var mod = _mods["AccuracyAndCritsJewel"];

            Assert.AreEqual(ModDomain.Jewel, mod.Domain);
            Assert.AreEqual(ModGenerationType.Suffix, mod.GenerationType);
            Assert.AreEqual("AccuracyAndCrits", mod.Group);
            Assert.AreEqual(false, mod.IsEssenceOnly);
            Assert.AreEqual("of Deadliness", mod.Name);
            Assert.AreEqual(1, mod.RequiredLevel);
            Assert.AreEqual(2, mod.SpawnTags.Length);
            Assert.AreEqual(2, mod.Stats.Length);

            var spawnTag = mod.SpawnTags[0];
            Assert.AreEqual(1, spawnTag.Count);
            Assert.AreEqual(false, spawnTag["not_dex"]);
            spawnTag = mod.SpawnTags[1];
            Assert.AreEqual(1, spawnTag.Count);
            Assert.AreEqual(true, spawnTag["default"]);

            var stat = mod.Stats[0];
            Assert.AreEqual("accuracy_rating_+%", stat.Id);
            Assert.AreEqual(10, stat.Max);
            Assert.AreEqual(6, stat.Min);
            stat = mod.Stats[1];
            Assert.AreEqual("critical_strike_chance_+%", stat.Id);
            Assert.AreEqual(10, stat.Max);
            Assert.AreEqual(6, stat.Min);
        }

        [TestMethod]
        public async Task JsonMod_LocalIncreasedAccuracy6()
        {
            await _initialization;
            var mod = _mods["LocalIncreasedAccuracy6"];

            Assert.AreEqual(ModDomain.Item, mod.Domain);
            Assert.AreEqual(ModGenerationType.Suffix, mod.GenerationType);
            Assert.AreEqual("IncreasedAccuracy", mod.Group);
            Assert.AreEqual(false, mod.IsEssenceOnly);
            Assert.AreEqual("of the Marksman", mod.Name);
            Assert.AreEqual(41, mod.RequiredLevel);
            Assert.AreEqual(3, mod.SpawnTags.Length);
            Assert.AreEqual(1, mod.Stats.Length);

            var spawnTag = mod.SpawnTags[0];
            Assert.AreEqual(1, spawnTag.Count);
            Assert.AreEqual(false, spawnTag["no_attack_mods"]);
            spawnTag = mod.SpawnTags[1];
            Assert.AreEqual(1, spawnTag.Count);
            Assert.AreEqual(true, spawnTag["weapon"]);
            spawnTag = mod.SpawnTags[2];
            Assert.AreEqual(1, spawnTag.Count);
            Assert.AreEqual(false, spawnTag["default"]);

            var stat = mod.Stats[0];
            Assert.AreEqual("local_accuracy_rating", stat.Id);
            Assert.AreEqual(200, stat.Max);
            Assert.AreEqual(166, stat.Min);
        }

        // make sure master crafted mods can't spawn through tags so matching their item classes is enough
        [TestMethod]
        public async Task MasterMods_NoSpawnTags()
        {
            await _initialization;
            foreach (var mod in _mods.Values)
            {
                if (mod.Domain == ModDomain.Master)
                {
                    AssertCantSpawn(mod);
                }
            }
        }

        // make sure essence mods can't spawn through tags so they need to be handled differently
        [TestMethod]
        public async Task EssenceMods_NoSpawnTags()
        {
            await _initialization;
            foreach (var mod in _mods.Values)
            {
                if (mod.IsEssenceOnly)
                {
                    AssertCantSpawn(mod);
                }
            }
        }

        // make sure signature mods can't spawn through tags 
        // so the spawn tags in JsonNpcMaster can simply be prepended
        [TestMethod]
        public async Task SignatureMods_NoSpawnTags()
        {
            await _initialization;
            foreach (var npcMaster in _npcMasters.Values)
            {
                // TryGetValue because Zana's signature mod is of domain Map. Those mods are not in _mods.
                JsonMod mod;
                if (_mods.TryGetValue(npcMaster.SignatureMod.Id, out mod))
                {
                    AssertCantSpawn(mod);
                }
            }
        }

        private void AssertCantSpawn(JsonMod mod)
        {
            foreach (var spawnTagDict in mod.SpawnTags)
            {
                if (spawnTagDict.Any())
                {
                    Assert.AreEqual(1, spawnTagDict.Count);
                    Assert.IsTrue(spawnTagDict.ContainsKey("default"));
                    Assert.IsFalse(spawnTagDict["default"]);
                }
            }
        }

        [TestMethod]
        public async Task GetMatchingMods_ChaosDamage()
        {
            await _initialization;
            var affixes = _modDatabase[ModGenerationType.Prefix];

            var chaosDamage = affixes.Single(a => a.Group == "ChaosDamage");
            Assert.AreEqual(2, chaosDamage.Mods.Count);
            Assert.AreEqual("ChaosDamageJewel", ((Mod) chaosDamage.Mods[0]).Id);
            Assert.AreEqual("StrIntMasterAddedChaosDamageCrafted", ((Mod) chaosDamage.Mods[1]).Id);

            var bowChaosDamage = chaosDamage.GetMatchingMods(
                Tags.Bow | Tags.TwoHandWeapon | Tags.Ranged, ItemClass.Bow).ToList();
            Assert.AreEqual(0, bowChaosDamage.Count);
            var jewelChaosDamage = chaosDamage.GetMatchingMods(
                Tags.Jewel | Tags.DexJewel | Tags.NotInt | Tags.NotStr, ItemClass.Jewel).ToList();
            Assert.AreEqual(1, jewelChaosDamage.Count);
            Assert.AreEqual("ChaosDamageJewel", ((Mod) jewelChaosDamage[0]).Id);
            var amuletChaosDamage = chaosDamage.GetMatchingMods(Tags.Amulet, ItemClass.Amulet).ToList();
            Assert.AreEqual(1, amuletChaosDamage.Count);
            Assert.AreEqual("StrIntMasterAddedChaosDamageCrafted", ((Mod) amuletChaosDamage[0]).Id);
        }

        [TestMethod]
        public async Task GetMatchingMods_ProjectileSpeed()
        {
            await _initialization;
            var affixes = _modDatabase[ModGenerationType.Suffix];
            var affix = affixes.Single(a => a.Group == "ProjectileSpeed");

            var quiver = affix.GetMatchingMods(Tags.Quiver, ItemClass.Quiver).ToList();
            Assert.AreEqual(6, quiver.Count);
            Assert.AreEqual("DexMasterProjectileSpeedCrafted", ((Mod) quiver[0]).Id);
            Assert.AreEqual("ProjectileSpeed1", ((Mod) quiver[1]).Id);
        }

        [TestMethod]
        public async Task GetMatchingMods_CausesBleeding()
        {
            await _initialization;
            var affixes = _modDatabase[ModGenerationType.Prefix];
            var affix = affixes.Single(a => a.Group == "CausesBleeding");

            var bow = affix.GetMatchingMods(
                Tags.Bow | Tags.TwoHandWeapon | Tags.Ranged, ItemClass.Bow).ToList();
            Assert.AreEqual(1, bow.Count);
            Assert.AreEqual("BleedOnHitGainedDexMasterVendorItem", ((Mod) bow[0]).Id);
        }

        // Make sure all possible Tags and ItemClasses are either known or purposefully unknown.

        [TestMethod]
        public async Task JsonMod_UnknownTags()
        {
            await _initialization;
            foreach (var mod in _mods.Values)
            {
                foreach (var spawnTagDict in mod.SpawnTags)
                {
                    foreach (var spawnTag in spawnTagDict.Keys)
                    {
                        Tags tag;
                        if (!TagsEx.TryParse(spawnTag, out tag))
                        {
                            Assert.IsTrue(UnknownTags.Contains(spawnTag), spawnTag + " unknown");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task JsonCraftingBenchOption_UnknownItemClasses()
        {
            await _initialization;
            foreach (var benchOption in _benchOptions)
            {
                foreach (var itemClass in benchOption.ItemClasses)
                {
                    ItemClass enumClass;
                    if (!ItemClassEx.TryParse(itemClass, out enumClass))
                    {
                        Assert.IsTrue(UnknownItemClasses.Contains(itemClass), itemClass + " unknown");
                    }
                }
            }
        }

        [TestMethod]
        public async Task JsonSignatureMod_UnknownTags()
        {
            await _initialization;
            foreach (var mod in _npcMasters.Values.Select(n => n.SignatureMod))
            {
                foreach (var spawnTagDict in mod.SpawnTags)
                {
                    foreach (var spawnTag in spawnTagDict.Keys)
                    {
                        Tags tag;
                        if (!TagsEx.TryParse(spawnTag, out tag))
                        {
                            Assert.IsTrue(UnknownTags.Contains(spawnTag), spawnTag + " unknown");
                        }
                    }
                }
            }
        }
    }
}