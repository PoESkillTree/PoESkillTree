using NUnit.Framework;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;

namespace PoESkillTree.GameModel.Tests.Modifiers
{
    [TestFixture]
    public class ModifierLocalityTesterTest
    {
        [TestCase("+20% to Quality", ExpectedResult = true)]
        [TestCase("+10 to maximum Life", ExpectedResult = false)]
        public bool IsLocalReturnsCorrectResultForGeneric(string modifier)
            => ModifierLocalityTester.IsLocal(modifier, Tags.Default);

        [TestCase("Adds 2 to 6 Physical Damage", ExpectedResult = true)]
        [TestCase("adds 2 to 6 physical damage", ExpectedResult = true)]
        [TestCase("Adds 2 to 6 Physical Damage to Attacks", ExpectedResult = false)]
        [TestCase("1% increased Physical Damage", ExpectedResult = true)]
        [TestCase("1% chance to maim on hit", ExpectedResult = true)]
        [TestCase("maim on hit", ExpectedResult = true)]
        [TestCase("Adds # to # Physical Damage to Attacks with this Weapon per 3 Player Levels", ExpectedResult = true)]
        [TestCase("Attacks with this Weapon Maim on hit", ExpectedResult = true)]
        public bool IsLocalReturnsCorrectResultForWeapon(string modifier)
            => ModifierLocalityTester.IsLocal(modifier, Tags.Weapon);

        [TestCase("+100 to Energy Shield", ExpectedResult = true)]
        [TestCase("-100 to Energy Shield", ExpectedResult = true)]
        [TestCase("10% increased Energy Shield", ExpectedResult = true)]
        [TestCase("10% reduced Energy Shield", ExpectedResult = true)]
        [TestCase("10% increased maximum Energy Shield", ExpectedResult = false)]
        [TestCase("1% increased Physical Damage", ExpectedResult = false)]
        public bool IsLocalReturnsCorrectResultForArmour(string modifier)
            => ModifierLocalityTester.IsLocal(modifier, Tags.Armour);

        [TestCase("+5% Chance to Block", ExpectedResult = true)]
        [TestCase("+25% Chance to Block Projectile Attack Damage", ExpectedResult = false)]
        public bool IsLocalReturnsCorrectResultForShield(string modifier)
            => ModifierLocalityTester.IsLocal(modifier, Tags.Shield);

        [TestCase("10% increased effect", ExpectedResult = true)]
        [TestCase("1% increased charge recovery", ExpectedResult = true)]
        [TestCase("Removes Burning on use", ExpectedResult = false)]
        public bool IsLocalReturnsCorrectResultForFlask(string modifier)
            => ModifierLocalityTester.IsLocal(modifier, Tags.Flask);

        [TestCase("+20% to Quality", ExpectedResult = true)]
        [TestCase("+10 to maximum Life", ExpectedResult = false)]
        public bool AffectsPropertiesReturnsCorrectResultForGeneric(string modifier)
            => ModifierLocalityTester.AffectsProperties(modifier, Tags.Default);

        [TestCase("Adds 2 to 6 Physical Damage", ExpectedResult = true)]
        [TestCase("Adds 2 to 6 Physical Damage to Attacks", ExpectedResult = false)]
        [TestCase("1% chance to maim on hit", ExpectedResult = false)]
        [TestCase("Adds # to # Physical Damage to Attacks with this Weapon per 3 Player Levels", ExpectedResult =
            true)]
        [TestCase("Attacks with this Weapon Maim on hit", ExpectedResult = false)]
        public bool AffectsPropertiesReturnsCorrectResultForWeapon(string modifier)
            => ModifierLocalityTester.AffectsProperties(modifier, Tags.Weapon);

        [TestCase("+100 to Energy Shield", ExpectedResult = true)]
        [TestCase("1% increased Physical Damage", ExpectedResult = false)]
        public bool AffectsPropertiesReturnsCorrectResultForArmour(string modifier)
            => ModifierLocalityTester.AffectsProperties(modifier, Tags.Armour);

        [TestCase("+5% Chance to Block", ExpectedResult = true)]
        [TestCase("+25% Chance to Block Projectile Attack Damage", ExpectedResult = false)]
        public bool AffectsPropertiesReturnsCorrectResultForShield(string modifier)
            => ModifierLocalityTester.AffectsProperties(modifier, Tags.Shield);

        [TestCase("10% increased effect", ExpectedResult = true)]
        [TestCase("1% increased charge recovery", ExpectedResult = false)]
        [TestCase("Removes Burning on use", ExpectedResult = false)]
        public bool AffectsPropertiesReturnsCorrectResultForFlask(string modifier)
            => ModifierLocalityTester.AffectsProperties(modifier, Tags.Flask);
    }
}