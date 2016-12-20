using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using System;
using static UnitTests.Compute.AttributeHelpers;
using static POESKillTree.Compute.ComputeGlobal;


namespace UnitTests.Compute
{
    [TestFixture]
    public class ArmorAndEvasion
    {
        [Test]
        public void ChanceToEvade_Test()
        {
            var c = new Computation();

            Assert.AreEqual(0f, c.ChanceToEvade(1, 0));
            Assert.AreEqual(0f, c.ChanceToEvade(0, 0));
            Assert.AreEqual(0f, c.ChanceToEvade(0, 100));
            Assert.AreEqual(42f, c.ChanceToEvade(1, 100));
        }

        [Test]
        public void PhysicalDamageReduction()
        {
            Assert.AreEqual(0f, ComputeGlobal.PhysicalDamageReduction(0, 0));
            Assert.AreEqual(0f, ComputeGlobal.PhysicalDamageReduction(1, 0));
            Assert.AreEqual(62.5f, ComputeGlobal.PhysicalDamageReduction(1, 100));
            Assert.AreEqual(62.5f, ComputeGlobal.PhysicalDamageReduction(2, 100));
            Assert.AreEqual(90f, ComputeGlobal.PhysicalDamageReduction(100, 100000));
        }

        [Test]
        public void Armor_Zero_Test()
        {
            var c = new Computation();

            c.Level = 1;
            var result = c.ArmorAndEvasion(0f, 0f, 0f);

            Assert.AreEqual(0, result.GetOrDefault("Armour: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Armour against Projectiles: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction against Projectiles: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Evasion Rating: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Melee Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Projectile Attacks: #%"));
        }

        [Test]
        public void Armor_Test()
        {
            var c = new Computation();

            c.Level = 1;
            c.Global.Add("Armour: #", 100f);
            c.Global.Add("+# to Armour", 100f);
            c.Global.Add("#% increased Armour", 10f);
            c.Global.Add("#% increased Armour against Projectiles", 10f);

            var result = c.ArmorAndEvasion(0f, 100f, 0f);

            var armor = result.GetOrDefault("Armour: #");
            Assert.AreEqual(320, armor); //shield armor isn't increased. bug? FIXME
            Assert.AreEqual(ComputeGlobal.RoundValue(ComputeGlobal.PhysicalDamageReduction(c.Level, armor), 0), result.GetOrDefault("Estimated Physical Damage reduction: #%"));

            var projArmor = result.GetOrDefault("Armour against Projectiles: #");
            Assert.AreEqual(340, projArmor);
            Assert.AreEqual(ComputeGlobal.RoundValue(ComputeGlobal.PhysicalDamageReduction(c.Level, projArmor), 0), result.GetOrDefault("Estimated Physical Damage reduction against Projectiles: #%"));

            Assert.AreEqual(0, result.GetOrDefault("Evasion Rating: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Melee Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Projectile Attacks: #%"));
        }

        [Test]
        public void Evasion_Test()
        {
            var c = new Computation();

            c.Level = 1;
            c.Global.Add("Evasion Rating: #", 100f);
            c.Global.Add("+# to Evasion Rating", 100f);
            c.Global.Add("#% increased Evasion Rating", 10f);

            var result = c.ArmorAndEvasion(0f, 0f, 100f);

            Assert.AreEqual(0, result.GetOrDefault("Armour: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Armour against Projectiles: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction against Projectiles: #%"));

            var evasion = result.GetOrDefault("Evasion Rating: #");
            Assert.AreEqual(320, evasion);
            Assert.AreEqual(c.ChanceToEvade(c.Level, evasion), result.GetOrDefault("Estimated chance to Evade Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Melee Attacks: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Projectile Attacks: #%"));
        }

        [Test]
        public void Evasion_Proj_Test()
        {
            var c = new Computation();

            c.Level = 1;
            c.Global.Add("Evasion Rating: #", 100f);
            c.Global.Add("+# to Evasion Rating", 100f);
            c.Global.Add("#% increased Evasion Rating", 10f);
            c.Global.Add("#% more chance to Evade Melee Attacks", 10f);
            c.Global.Add("#% less chance to Evade Projectile Attacks", 10f);

            var result = c.ArmorAndEvasion(0f, 0f, 100f);

            Assert.AreEqual(0, result.GetOrDefault("Armour: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction: #%"));
            Assert.AreEqual(0, result.GetOrDefault("Armour against Projectiles: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated Physical Damage reduction against Projectiles: #%"));

            Assert.AreEqual(320, result.GetOrDefault("Evasion Rating: #"));
            Assert.AreEqual(0, result.GetOrDefault("Estimated chance to Evade Attacks: #%"));
            var meleeEvasion = result.GetOrDefault("Estimated chance to Evade Melee Attacks: #%");
            Assert.AreEqual(RoundValue(IncreaseValueByPercentage(c.ChanceToEvade(c.Level, 320), 10f), 0), result.GetOrDefault("Estimated chance to Evade Melee Attacks: #%"));
            Assert.AreEqual(RoundValue(IncreaseValueByPercentage(c.ChanceToEvade(c.Level, 320), -10f), 0), result.GetOrDefault("Estimated chance to Evade Projectile Attacks: #%"));
        }
    }
}
