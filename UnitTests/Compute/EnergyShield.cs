using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using static UnitTests.Compute.AttributeHelpers;


namespace UnitTests.Compute
{
    [TestFixture]
    public class EnergyShield
    {
        [Test]
        public void EnergyShield_Zero_Test()
        {
            var c = new Computation();

            var result = c.EnergyShield(0f);

            Assert.AreEqual(0f, result.EnergyShield);
            Assert.AreEqual(0f, result.ShieldArmor);
            Assert.AreEqual(0f, result.ShieldEvasion);
            Assert.AreEqual(0f, result.Mana);
            Assert.AreEqual(0f, result.IncreasedMana);
        }

        [Test]
        public void Mana_Test()
        {
            var c = new Computation();
            c.Global.Add("+# to maximum Mana", 100f);
            c.Global.Add("#% increased maximum Mana", 1f);

            var result = c.EnergyShield(0f);

            Assert.AreEqual(0f, result.EnergyShield);
            Assert.AreEqual(0f, result.ShieldArmor);
            Assert.AreEqual(0f, result.ShieldEvasion);
            Assert.AreEqual(100f * 1.01f, result.Mana);
            Assert.AreEqual(1f, result.IncreasedMana);
        }

        [Test]
        public void EnergyShield_Test()
        {
            var c = new Computation();
            c.Global.Add("+# to maximum Energy Shield", 1f);
            c.Global.Add("Energy Shield: #", 1f);
            c.Global.Add("#% increased maximum Energy Shield", 100f);
            c.Global.Add("#% more maximum Energy Shield", 20f);


            var result = c.EnergyShield(0f);

            Assert.AreEqual((1f + 1f) * 2f * 1.2f, result.EnergyShield);
            Assert.AreEqual(0f, result.ShieldArmor);
            Assert.AreEqual(0f, result.ShieldEvasion);
            Assert.AreEqual(0f, result.Mana);
            Assert.AreEqual(0f, result.IncreasedMana);
        }

        [Test]
        public void EnergyShield_Shield_Test()
        {
            var c = new Computation();
            c.Global.Add("+# to maximum Energy Shield", 1f);
            c.Global.Add("Energy Shield: #", 2f);
            c.Global.Add("#% increased maximum Energy Shield", 100f);
            c.Global.Add("#% more maximum Energy Shield", 20f);
            c.Global.Add("#% increased Energy Shield from equipped Shield", 10f);
            c.Global.Add("#% increased Armour from equipped Shield", 10f);
            c.Global.Add("#% increased Defences from equipped Shield", 10f);

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Energy Shield: #", 100f);
            c.OffHand.Attributes.Add("Evasion Rating: #", 10f);
            c.OffHand.Attributes.Add("Armour: #", 10f);

            var result = c.EnergyShield(0f);

            //es = ((es * inc) + (fromShield * incShield)) * more
            var incedES = ((1f + 2f) * 2f);
            var shieldES = (100f * (.1f + .1f));
            EqualWithinMargin((incedES + shieldES) * 1.2f, result.EnergyShield);
            Assert.AreEqual(10f * .2f, result.ShieldArmor);
            Assert.AreEqual(10f * .1f, result.ShieldEvasion);
        }

        [Test]
        public void EnergyShield_Shield_Acro_Test()
        {
            var c = new Computation();
            c.Global.Add("+# to maximum Energy Shield", 1f);
            c.Global.Add("Energy Shield: #", 2f);
            c.Global.Add("#% increased maximum Energy Shield", 100f);
            c.Global.Add("#% more maximum Energy Shield", 20f);
            c.Global.Add("#% increased Energy Shield from equipped Shield", 10f);
            c.Global.Add("#% increased Armour from equipped Shield", 10f);
            c.Global.Add("#% increased Defences from equipped Shield", 10f);

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Energy Shield: #", 100f);
            c.OffHand.Attributes.Add("Evasion Rating: #", 10f);
            c.OffHand.Attributes.Add("Armour: #", 10f);

            var result = c.EnergyShield(50f);

            //es = ((es * inc) + (fromShield * incShield)) * more
            var incedES = ((1f + 2f) * 2f);
            var shieldES = (100f * (.1f + .1f));
            EqualWithinMargin((incedES + shieldES) * 1.2f * .5f, result.EnergyShield);
            Assert.AreEqual(10f * .2f, result.ShieldArmor);
            Assert.AreEqual(10f * .1f, result.ShieldEvasion);
        }

        [Test]
        public void Mana_EldritchBattery_Test()
        {
            var increasedMana = .3f;
            var c = new Computation();
            c.Global.Add("+# to maximum Mana", 100f);
            c.Global.Add("#% increased maximum Mana", increasedMana * 100);
            c.Global.Add("+# to maximum Energy Shield", 1f);
            c.Global.Add("Energy Shield: #", 2f);
            c.Global.Add("#% increased maximum Energy Shield", 100f);
            c.Global.Add("#% more maximum Energy Shield", 20f);
            c.Global.Add("#% increased Energy Shield from equipped Shield", 10f);
            c.Global.Add("#% increased Armour from equipped Shield", 10f);
            c.Global.Add("#% increased Defences from equipped Shield", 10f);
            c.Global.Add("Converts all Energy Shield to Mana", 0f);

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Energy Shield: #", 100f);

            var result = c.EnergyShield(0f);

            //es = ((es * inc) + (fromShield * incShield)) * more
            Assert.AreEqual(0f, result.EnergyShield);
            var mana = (100f * (1 + increasedMana));
            //es is multiplied by %inc mana in this case
            var incedES = (1f + 2f) * (2f + increasedMana);
            var shieldES = (100f * (.1f + .1f));
            var es = (incedES + shieldES) * 1.2f;
            Assert.AreEqual(mana + es, result.Mana);
        }

        [Test]
        public void Mana_EldritchBattery_Acro_Test()
        {
            var increasedMana = .3f;
            var c = new Computation();
            c.Global.Add("+# to maximum Mana", 100f);
            c.Global.Add("#% increased maximum Mana", increasedMana * 100);
            c.Global.Add("+# to maximum Energy Shield", 1f);
            c.Global.Add("Energy Shield: #", 2f);
            c.Global.Add("#% increased maximum Energy Shield", 100f);
            c.Global.Add("#% more maximum Energy Shield", 20f);
            c.Global.Add("#% increased Energy Shield from equipped Shield", 10f);
            c.Global.Add("#% increased Armour from equipped Shield", 10f);
            c.Global.Add("#% increased Defences from equipped Shield", 10f);
            c.Global.Add("Converts all Energy Shield to Mana", 0f);

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Energy Shield: #", 100f);

            var lessES = 50f;

            var result = c.EnergyShield(lessES);

            //es = ((es * inc) + (fromShield * incShield)) * more
            Assert.AreEqual(0f, result.EnergyShield);
            var mana = (100f * (1 + increasedMana));
            //es is multiplied by %inc mana in this case
            var incedES = (1f + 2f) * (2f + increasedMana);
            var shieldES = (100f * (.1f + .1f));
            var es = (incedES + shieldES) * 1.2f;
            Assert.AreEqual(mana + (es * lessES / 100), result.Mana);
        }
    }
}
