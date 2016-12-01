using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using static UnitTests.Compute.AttributeHelpers;

namespace UnitTests.Compute
{
    [TestFixture]
    public class BlockTest
    {

        [Test]
        public void MaxBlock()
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            Assert.AreEqual(75, c.GetMaxBlock());
        }

        [Test]
        public void MaxBlock_add()
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add("+#% to maximum Block Chance", 10f);
            Assert.AreEqual(85, c.GetMaxBlock());
        }

        [Test]
        public void Block_DualWield()
        {
            var c = new Computation();

            c.IsDualWielding = true;
            var result = c.Block(false);

            CheckEquality(new[] { 15, 75f }, result["Chance to Block Attacks: #%"]);
        }

        [Test]
        public void MaxBlock_Bonus()
        {
            var c = new Computation();

            c.IsDualWielding = true;
            c.Global.Add("+#% to maximum Block Chance", 1f);
            var result = c.Block(false);

            CheckEquality(new[] { 15, 76f }, result["Chance to Block Attacks: #%"]);
        }

        [Test]
        public void Block_Shield()
        {
            var c = new Computation();

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Chance to Block: #%", 1f);
            c.Global.Add("#% additional Chance to Block with Shields", 1f);
            c.Global.Add("#% additional Chance to Block Spells with Shields", 1f);
            var result = c.Block(true);

            CheckEquality(new[] { 2, 75f }, result["Chance to Block Attacks: #%"]);
            CheckEquality(new[] { 1, 75f }, result["Chance to Block Spells: #%"]);
            CheckEquality(new[] { 2, 75f }, result["Chance to Block Projectile Attacks: #%"]);
        }

        [Test]
        public void Block_Shield_Acro()
        {
            var c = new Computation();

            c.OffHand = new Weapon(WeaponHand.Off);
            c.OffHand.Attributes.Add("Chance to Block: #%", 2f);
            c.Global.Add("#% additional Chance to Block with Shields", 2f);
            c.Global.Add("#% additional Chance to Block Spells with Shields", 2f);
            c.Acrobatics = true;
            c.Global.Add("#% Chance to Dodge Attacks. #% less Armour and Energy Shield, #% less Chance to Block Spells and Attacks", new[] { 0f, 0f, 50 });
            var result = c.Block(true);

            CheckEquality(new[] { 2, 75f }, result["Chance to Block Attacks: #%"]);
            CheckEquality(new[] { 1, 75f }, result["Chance to Block Spells: #%"]);
            CheckEquality(new[] { 2, 75f }, result["Chance to Block Projectile Attacks: #%"]);
        }

        [Test]
        public void Block_Staff()
        {
            var c = new Computation();

            c.MainHand = new Weapon(WeaponHand.Main);
            c.MainHand.Nature = new DamageNature { WeaponType = WeaponType.Staff };
            c.MainHand.Attributes.Add("#% Chance to Block", 1f);
            c.Global.Add("#% additional Block Chance With Staves", 1f);
            var result = c.Block(false);

            CheckEquality(new[] { 2, 75f }, result["Chance to Block Attacks: #%"]);
        }
    }
}
