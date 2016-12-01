using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.SkillTreeFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Compute
{
    [TestFixture]
    public class ComputationTest
    {
        void CheckEquality(IEnumerable<float> expected, IEnumerable<float> actual)
        {
            for(int i = 0; i < expected.Count(); i ++)
                Assert.AreEqual(expected.Skip(i).First(), actual.Skip(i).First());
        }

        [Test]
        public void Single_Resistance_Test()
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add("+#% to Fire Resistance", 60f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 60f, 60f }, attSet["Fire Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to {type} Resistance", 135f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 75f, 135f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Cruel_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to {type} Resistance", 60);
            c.Resistances(attSet, Difficulty.Cruel, false);

            CheckEquality(new[] { 40f, 40f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Merc_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to {type} Resistance", 60);
            c.Resistances(attSet, Difficulty.Merciless, false);

            CheckEquality(new[] { 0f, 0f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Merc_Negative_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Resistances(attSet, Difficulty.Merciless, false);

            CheckEquality(new[] { -60f, -60f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Max_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to {type} Resistance", 135f);
            c.Global.Add($"+#% to maximum {type} Resistance", 10f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 85f, 135f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void Chaos_All_Test()
        {
            var c = new Computation();
            var type = "Chaos";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to All Resistances", 135f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 0f, 0f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void Chaos_Innoc_Test()
        {
            var c = new Computation();
            c.ChaosInoculation = true;
            var type = "Chaos";

            var attSet = new AttributeSet();
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 100f, 100f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Dual_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to {type} and Lightning Resistances", 10f);
            c.Global.Add($"+#% to Lightning and {type} Resistances", 10f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 20f, 20f }, attSet[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Shield_Test()
        {
            var c = new Computation();
            var attSet = new AttributeSet();
            c.Global.Add($"+#% Elemental Resistances while holding a Shield", 1f);
            c.Resistances(attSet, Difficulty.Normal, true);

            CheckEquality(new[] { 1f, 1f }, attSet["Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 1f, 1f }, attSet["Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 1f, 1f }, attSet["Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, attSet["Chaos Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Shield_False_Test()
        {
            var c = new Computation();
            var attSet = new AttributeSet();
            c.Global.Add($"+#% Elemental Resistances while holding a Shield", 1f);
            c.Resistances(attSet, Difficulty.Normal, false);

            CheckEquality(new[] { 0f, 0f }, attSet["Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, attSet["Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, attSet["Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, attSet["Chaos Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Full_Test()
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add($"+#% to Fire Resistance", 1f);
            c.Global.Add($"+#% to Cold Resistance", 2f);
            c.Global.Add($"+#% to Lightning Resistance", 40f);
            c.Global.Add($"+#% to Fire and Lightning Resistances", 10f);
            c.Global.Add($"+#% to Lightning and Cold Resistances", 10f);
            c.Global.Add($"+#% to Fire and Cold Resistances", 10f);
            c.Global.Add($"+#% to all Elemental Resistances", 20f);
            c.Global.Add($"+#% to Chaos Resistance", 20f);
            c.Global.Add($"+#% to maximum Lightning Resistance", 1f);
            c.Global.Add($"+#% Elemental Resistances while holding a Shield", 1f);

            c.Resistances(attSet, Difficulty.Normal, true);

            CheckEquality(new[] { 42f, 42f }, attSet[$"Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 43f, 43f }, attSet[$"Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 76f, 81f }, attSet[$"Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 20f, 20f }, attSet[$"Chaos Resistance: #% (#%)"]);
        }

    }
}
