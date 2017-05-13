using System;
using System.Collections.Generic;
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
        private Task _initialization;

        private Dictionary<string, JsonMod> _mods;
        private JsonCraftingBenchOption[] _masterMods;
        private ModDatabase _modDatabase;

        [TestInitialize]
        public void TestInitialize()
        {
            _initialization = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var loader = new RePoELoader(new HttpClient(), false);
            _mods = await loader.LoadAsync<Dictionary<string, JsonMod>>("mods");
            _masterMods = await loader.LoadAsync<JsonCraftingBenchOption[]>("crafting_bench_options");
            _modDatabase = new ModDatabase(_mods, _masterMods);
        }

        [TestMethod]
        public async Task JsonMod_AccuracyAndCritsJewel()
        {
            await _initialization;
            var mod = _mods["AccuracyAndCritsJewel"];

            Assert.AreEqual(ModDomain.Jewel, mod.Domain);
            Assert.AreEqual(ModType.Suffix, mod.GenerationType);
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
            Assert.AreEqual(ModType.Suffix, mod.GenerationType);
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
        public async Task ModDatabase_MasterMods_NoSpawnTags()
        {
            await _initialization;
            foreach (var modType in Util.GetEnumValues<ModType>())
            {
                var affixes = _modDatabase[modType];
                foreach (var affix in affixes)
                {
                    foreach (Mod mod in affix.Mods)
                    {
                        if (mod.Domain != ModDomain.Master)
                        {
                            continue;
                        }
                        foreach (var spawnTagDict in mod.JsonMod.SpawnTags)
                        {
                            if (spawnTagDict.Any())
                            {
                                Assert.AreEqual(1, spawnTagDict.Count);
                                Assert.IsTrue(spawnTagDict.ContainsKey("default"));
                                Assert.IsFalse(spawnTagDict["default"]);
                            }
                        }
                    }
                }
            }
        }

        // make sure essence mods can't spawn through tags so they need to be handled differently
        [TestMethod]
        public async Task ModDatabase_EssenceMods_NoSpawnTags()
        {
            await _initialization;
            foreach (var modType in Util.GetEnumValues<ModType>())
            {
                var affixes = _modDatabase[modType];
                foreach (var affix in affixes)
                {
                    foreach (Mod mod in affix.Mods)
                    {
                        if (!mod.IsEssenceOnly)
                        {
                            continue;
                        }
                        foreach (var spawnTagDict in mod.JsonMod.SpawnTags)
                        {
                            if (spawnTagDict.Any())
                            {
                                Assert.AreEqual(1, spawnTagDict.Count);
                                Assert.IsTrue(spawnTagDict.ContainsKey("default"));
                                Assert.IsFalse(spawnTagDict["default"]);
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public async Task GetMatchingMods_ChaosDamage()
        {
            await _initialization;
            var affixes = _modDatabase[ModType.Prefix];

            var chaosDamage = affixes.Single(a => a.Group == "ChaosDamage");
            Assert.AreEqual(2, chaosDamage.Mods.Count);
            Assert.AreEqual("ChaosDamageJewel", chaosDamage.Mods[0].Id);
            Assert.AreEqual("StrIntMasterAddedChaosDamageCrafted", chaosDamage.Mods[1].Id);

            var bowChaosDamage = chaosDamage.GetMatchingMods(ModDomain.Item,
                Tags.Bow | Tags.TwoHandWeapon | Tags.Ranged, ItemClass.Bow).ToList();
            Assert.AreEqual(0, bowChaosDamage.Count);
            var jewelChaosDamage = chaosDamage.GetMatchingMods(ModDomain.Jewel,
                Tags.Jewel | Tags.DexJewel | Tags.NotInt | Tags.NotStr, ItemClass.Jewel).ToList();
            Assert.AreEqual(1, jewelChaosDamage.Count);
            Assert.AreEqual("ChaosDamageJewel", jewelChaosDamage[0].Id);
            var amuletChaosDamage = chaosDamage.GetMatchingMods(ModDomain.Item,
                Tags.Amulet, ItemClass.Amulet).ToList();
            Assert.AreEqual(1, amuletChaosDamage.Count);
            Assert.AreEqual("StrIntMasterAddedChaosDamageCrafted", amuletChaosDamage[0].Id);
        }

        [TestMethod]
        public async Task GetMatchingMods_ProjectileSpeed()
        {
            await _initialization;
            var affixes = _modDatabase[ModType.Suffix];
            var affix = affixes.Single(a => a.Group == "ProjectileSpeed");

            var quiver = affix.GetMatchingMods(ModDomain.Item, Tags.Quiver, ItemClass.Quiver).ToList();
            Assert.AreEqual(6, quiver.Count);
            Assert.AreEqual("DexMasterProjectileSpeedCrafted", quiver[0].Id);
            Assert.AreEqual("ProjectileSpeed1", quiver[1].Id);
        }
    }
}