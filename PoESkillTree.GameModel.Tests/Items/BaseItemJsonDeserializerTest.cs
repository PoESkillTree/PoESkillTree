using Newtonsoft.Json.Linq;
using NUnit.Framework;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.GameModel.Tests.Items
{
    [TestFixture]
    public class BaseItemJsonDeserializerTest
    {
        [Test]
        public void DeserializeReturnsCorrectResultForArchonKiteShield()
        {
            var definitions = DeserializeAll();

            Assert.That(definitions.BaseItems, Has.One.Items);
        }

        private static BaseItemDefinitions DeserializeAll()
        {
            /* Base items in base_items.json: (from game version 3.4.0)
             * ['Archon Kite Shield']
             */
            var json = JObject.Parse(TestUtils.ReadDataFile("base_items.json"));
            return BaseItemJsonDeserializer.Deserialize(json);
        }
    }
}