using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace PoESkillTree.GameModel.Modifiers
{
    [TestFixture]
    public class ModifierJsonDeserializerTest
    {
        [Test]
        public void DeserializeReturnsCorrectModifiers()
        {
            var expected = new[]
            {
                "AllResistancesImplicitShield3",
                "MovementVelocityPenaltyLightShieldImplicit",
                "AbyssAccuracyRatingJewel1",
                "ColdDamagePrefixOnTwoHandWeapon1",
            };

            var definitions = DeserializeAll();

            definitions.Modifiers.Should().Equal(expected, (d, s) => d.Id == s);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForAbyssAccuracyRatingJewel1()
        {
            var expected = new ModifierDefinition(
                "AbyssAccuracyRatingJewel1",
                ModDomain.AbyssJewel,
                ModGenerationType.Suffix,
                new []
                {
                    new ModifierSpawnWeight("abyss_jewel_melee", 0), 
                    new ModifierSpawnWeight("abyss_jewel_ranged", 0), 
                    new ModifierSpawnWeight("default", 0), 
                },
                new []
                {
                    new CraftableStat("accuracy_rating", 10, 30), 
                });

            var definitions = DeserializeAll();

            var definition = definitions.GetModifierById("AbyssAccuracyRatingJewel1");
            definition.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void DeserializeReturnsCorrectResultForColdDamagePrefixOnTwoHandWeapon1()
        {
            var expected = new ModifierDefinition(
                "ColdDamagePrefixOnTwoHandWeapon1",
                ModDomain.Item,
                ModGenerationType.Prefix,
                new []
                {
                    new ModifierSpawnWeight("no_elemental_damage_mods", 0), 
                    new ModifierSpawnWeight("no_caster_mods", 0), 
                    new ModifierSpawnWeight("staff", 250), 
                    new ModifierSpawnWeight("default", 0), 
                },
                new []
                {
                    new CraftableStat("cold_damage_+%", 15, 29), 
                });

            var definitions = DeserializeAll();

            var definition = definitions.GetModifierById("ColdDamagePrefixOnTwoHandWeapon1");
            definition.Should().BeEquivalentTo(expected);
        }

        private static ModifierDefinitions DeserializeAll()
        {
            /* Mods in mods.json: (from game version 3.6.0)
             * AllResistancesImplicitShield3, MovementVelocityPenaltyLightShieldImplicit, BreachDomainBossMod1,
             * MapAtlasWeaponsDropAnimated, AbyssAccuracyRatingJewel1, ColdDamagePrefixOnTwoHandWeapon1
             */
            var modJson =  JObject.Parse(TestUtils.ReadDataFile("mods.json"));
            return ModifierJsonDeserializer.Deserialize(modJson);
        }
    }
}