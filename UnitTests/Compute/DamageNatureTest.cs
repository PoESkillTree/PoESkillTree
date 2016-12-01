using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POESKillTree.Compute;

namespace UnitTests
{
    [TestFixture]
    public class DamageNature_Test
    {
        [Test]
        public void Base_Copy_Test()
        {

            var orig = new DamageNature
            {
                Form = DamageForm.DoT,
                Source = DamageSource.Spell,
                Type = DamageType.Cold,
                WeaponHand = WeaponHand.Main,
                WeaponType = WeaponType.Melee
            };
            var result = new DamageNature(orig);
            Assert.AreEqual(orig.Form, result.Form);
            Assert.AreEqual(orig.Source, result.Source);
            Assert.AreEqual(orig.Type, result.Type);
            Assert.AreEqual(orig.WeaponHand, result.WeaponHand);
            Assert.AreEqual(orig.WeaponType, result.WeaponType);

            Assert.IsTrue(orig.Matches(result));
            result.Type = DamageType.Chaos;
            Assert.IsTrue(orig.MatchesExceptType(result));
        }

        [Test]
        public void Gained_RegainMod_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("Gain #% of Physical Damage as Extra Cold Damage", new[] { 1f }.ToList());
            var gained = Damage.Gained.Create(kvp);
            Assert.IsNotNull(gained);
            Assert.AreEqual(1, gained.Percent);
            Assert.AreEqual(DamageType.Cold, gained.To);
        }

        [Test]
        public void Gained_RegainAddedMod_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% of Physical Damage Added as Cold Damage", new[] { 1f }.ToList());
            var gained = Damage.Gained.Create(kvp);
            Assert.IsNotNull(gained);
            Assert.AreEqual(1, gained.Percent);
            Assert.AreEqual(DamageType.Cold, gained.To);
        }

        [Test]
        public void Gained_Neither_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("unmatchingString", new[] { 1f }.ToList());
            var gained = Damage.Gained.Create(kvp);
            Assert.IsNull(gained);
        }

        //static Regex ReIncreasedAll = new Regex("^#% (increased|reduced) Damage$");
        //static Regex ReIncreasedAllWithWeaponType = new Regex("#% (increased|reduced) Damage with (.+)$");
        //static Regex ReIncreasedType = new Regex("^#% (increased|reduced) (.+) Damage$");
        //static Regex ReIncreasedTypeWithWeaponTypeOrHand = new Regex("#% (increased|reduced) (.+) Damage with (.+)$");
        //static Regex ReIncreasedWithSource = new Regex("#% (increased|reduced) (.+) Damage with (Spells|Attacks|Weapons)$");



        [Test]
        public void Increased_Type_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Cold Damage", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(DamageType.Cold, result.Type);
        }

        [Test]
        public void Increased_Reduced_Type_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Cold Damage", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(DamageType.Cold, result.Type);
        }

        [Test]
        public void Increased_All_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Damage", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(DamageType.Any, result.Type);
        }

        [Test]
        public void Increased_All_Reduced_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Damage", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(DamageType.Any, result.Type);
        }

        [Test]
        public void Increased_Weapon_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Damage with Swords", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(WeaponType.Sword, result.WeaponType);
            Assert.AreEqual(DamageType.Any, result.Type);
        }

        [Test]
        public void Increased_Weapon_Reduced_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Damage with Axes", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(WeaponType.Axe, result.WeaponType);
            Assert.AreEqual(DamageType.Any, result.Type);
        }

        [Test]
        public void Increased_Hand_Weapon_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Fire Damage with Main Hand", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(WeaponHand.Main, result.WeaponHand);
            Assert.AreEqual(DamageType.Fire, result.Type);
        }

        [Test]
        public void Increased_Hand_Weapon_Reduced_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Cold Damage with Main Hand", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(WeaponHand.Main, result.WeaponHand);
            Assert.AreEqual(DamageType.Cold, result.Type);
        }

        [Test]
        public void Increased_Hand_Weapon_Test2()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Fire Damage with Swords", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(WeaponType.Sword, result.WeaponType);
            Assert.AreEqual(DamageType.Fire, result.Type);
        }

        [Test]
        public void Increased_Hand_Weapon_Reduced_Test2()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Cold Damage with Swords", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(WeaponType.Sword, result.WeaponType);
            Assert.AreEqual(DamageType.Cold, result.Type);
        }

        [Test]
        public void Increased_Source_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% increased Cold Damage with Attacks", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(1f, result.Percent);
            Assert.AreEqual(DamageSource.Attack, result.Source);
            Assert.AreEqual(DamageType.Cold, result.Type);
        }

        [Test]
        public void Increased_Source_Reduced_Test()
        {
            var kvp = new KeyValuePair<string, List<float>>("#% reduced Fire Damage with Spells", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.AreEqual(-1f, result.Percent);
            Assert.AreEqual(DamageSource.Spell, result.Source);
            Assert.AreEqual(DamageType.Fire, result.Type);
        }

        [Test]
        public void Increased_NoMatch()
        {
            var kvp = new KeyValuePair<string, List<float>>("unmatched", new[] { 1f }.ToList());
            var result = Damage.Increased.Create(kvp);
            Assert.IsNull(result);
        }
    }
}
