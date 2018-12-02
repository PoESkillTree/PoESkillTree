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

            var definition = definitions.GetBaseItemById("Metadata/Items/Armours/Shields/ShieldStrInt13");
            Assert.AreEqual("Metadata/Items/Armours/Shields/ShieldStrInt13", definition.MetadataId);
            Assert.AreEqual("Archon Kite Shield", definition.Name);
            Assert.AreEqual(ItemClass.Shield, definition.ItemClass);
            Assert.AreEqual(
                new[] { "str_int_armour", "str_int_shield", "shield", "armour", "default" },
                definition.RawTags);
            Assert.AreEqual(Tags.StrIntShield | Tags.StrIntArmour | Tags.Shield | Tags.Armour, definition.Tags);
            Assert.AreEqual(
                new[] { new Property("armour", 156), new Property("block", 22), new Property("energy_shield", 30) },
                definition.Properties);
            Assert.AreEqual(new Requirements(68, 0, 85, 85), definition.Requirements);
            Assert.AreEqual(
                new[]
                {
                    new CraftableStat("from_armour_movement_speed_+%", -3, -3),
                    new CraftableStat("base_resist_all_elements_%", 12, 12),
                },
                definition.ImplicitModifiers);
            Assert.AreEqual(3, definition.InventoryHeight);
            Assert.AreEqual(2, definition.InventoryWidth);
            Assert.AreEqual(68, definition.DropLevel);
            Assert.AreEqual(ReleaseState.Released, definition.ReleaseState);
            Assert.AreEqual("Art/2DItems/Armours/Shields/ShieldStrInt5.dds", definition.VisualIdentity);
        }

        private static BaseItemDefinitions DeserializeAll()
        {
            /* Base items in base_items.json: (from game version 3.4.0)
             * ['Archon Kite Shield', 'Mystery Leaguestone']
             */
            var itemJson = JObject.Parse(TestUtils.ReadDataFile("base_items.json"));
            var modJson =  JObject.Parse(TestUtils.ReadDataFile("mods.json"));
            return BaseItemJsonDeserializer.Deserialize(itemJson, modJson);
        }
    }
}