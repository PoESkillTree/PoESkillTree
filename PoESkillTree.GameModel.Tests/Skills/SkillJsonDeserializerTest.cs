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
            Assert.AreEqual("Frenzy", definition.ActiveSkill.DisplayName);
            Assert.AreEqual(new[] { Keyword.Attack, Keyword.Projectile, Keyword.Melee },
                definition.ActiveSkill.Keywords);
            Assert.IsFalse(definition.ActiveSkill.ProvidesBuff);
        }

        private static string GetDataFilePath(string filename)
            => TestContext.CurrentContext.TestDirectory + "/Data/" + filename;
    }
}