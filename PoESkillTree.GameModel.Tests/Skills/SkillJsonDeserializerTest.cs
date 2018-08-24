using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.GameModel.Tests.Skills
{
    [TestFixture]
    public class SkillJsonDeserializerTest
    {
        [Test]
        public void DeserializeReturnsCorrectResultForFrenzy()
        {
            var gemJson = JObject.Parse(File.ReadAllText(GetDataFilePath("frenzyGem.json")));
            var gemTooltipJson = JObject.Parse(File.ReadAllText(GetDataFilePath("frenzyGemTooltip.json")));

            var definitions = SkillJsonDeserializer.Deserialize(gemJson, gemTooltipJson);

            Assert.That(definitions.Skills, Has.One.Items);
            var definition = definitions.GetSkillById("Frenzy");
            Assert.AreEqual("Frenzy", definition.Id);
            Assert.AreEqual(0, definition.NumericId);
            Assert.IsFalse(definition.IsSupport);

            var activeSkill = definition.ActiveSkill;
            Assert.AreEqual("Frenzy", activeSkill.DisplayName);
            Assert.That(activeSkill.ActiveSkillTypes, Has.Exactly(11).Items);
            Assert.AreEqual(new[] { Keyword.Attack, Keyword.Projectile, Keyword.Melee }, activeSkill.Keywords);
            Assert.IsFalse(activeSkill.ProvidesBuff);

            Assert.That(definition.Levels, Has.Exactly(2).Items);
            var level1 = definition.Levels[1];
            Assert.AreEqual(1.1, level1.DamageEffectiveness);
            Assert.AreEqual(1.1, level1.DamageMultiplier);
            Assert.AreEqual(10, level1.ManaCost);
            Assert.AreEqual(16, level1.RequiredLevel);
            Assert.AreEqual(41, level1.RequiredDexterity);
            Assert.AreEqual(0, level1.RequiredIntelligence);
            Assert.AreEqual(new[] { new UntranslatedStat("attack_speed_+%", 500) }, level1.QualityStats);
            Assert.AreEqual(new UntranslatedStat("physical_damage_+%_per_frenzy_charge", 5), level1.Stats[0]);
            var level20 = definition.Levels[20];
            Assert.AreEqual(1.37, level20.DamageEffectiveness);
            Assert.AreEqual(10, level20.ManaCost);
            Assert.AreEqual(155, level20.RequiredDexterity);
            Assert.AreEqual(0, level20.RequiredIntelligence);
            Assert.AreEqual(new[] { new UntranslatedStat("attack_speed_+%", 500) }, level20.QualityStats);
            Assert.AreEqual(new UntranslatedStat("physical_damage_+%_per_frenzy_charge", 5), level20.Stats[0]);

            var tooltip = level20.Tooltip;
            Assert.AreEqual("Frenzy", tooltip.Name);
            Assert.AreEqual(new[]
            {
                new TranslatedStat("Attack, Melee, Bow"),
                new TranslatedStat("Level: {0} (Max)", 20),
                new TranslatedStat("Mana Cost: {0}", 10),
                new TranslatedStat("Cast Time: {0} sec", 1),
                new TranslatedStat("Damage Effectiveness: {0}%", 137),
            }, tooltip.Properties);
            Assert.AreEqual(new[] { new TranslatedStat("Requires Level {0}, {1} Dex", 70, 155) }, tooltip.Requirements);
            Assert.That(tooltip.Description, Has.One.StartsWith("Performs an attack that gives"));
            Assert.AreEqual(new[] { new TranslatedStat("{0}% increased Attack Speed", 0.5) }, tooltip.QualityStats);
            Assert.AreEqual(new[]
            {
                new TranslatedStat("Deals {0}% of Base Attack Damage", 136.6),
                new TranslatedStat("{0}% increased Physical Damage per Frenzy Charge", 5),
                new TranslatedStat("{0}% increased Attack Speed per Frenzy Charge", 5),
            }, tooltip.Stats);
        }

        private static string GetDataFilePath(string filename)
            => TestContext.CurrentContext.TestDirectory + "/Data/" + filename;
    }
}