using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using static UnitTests.Compute.AttributeHelpers;

namespace UnitTests.Compute
{
    [TestFixture]
    public class ResistanceCalcTest
    {
        [Test]
        public void Single_Resistance_Test()
        {
            var c = new Computation();

            c.Global.Add("+#% to Fire Resistance", 60f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 60f, 60f }, result["Fire Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Test()
        {
            var c = new Computation();
            var type = "Fire";

            c.Global.Add($"+#% to {type} Resistance", 135f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 75f, 135f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Cruel_Test()
        {
            var c = new Computation();
            var type = "Fire";

            c.Global.Add($"+#% to {type} Resistance", 60);
            var result = c.Resistances(Difficulty.Cruel, false);

            CheckEquality(new[] { 40f, 40f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Merc_Test()
        {
            var c = new Computation();
            var type = "Fire";

            c.Global.Add($"+#% to {type} Resistance", 60);
            var result = c.Resistances(Difficulty.Merciless, false);

            CheckEquality(new[] { 0f, 0f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Merc_Negative_Test()
        {
            var c = new Computation();
            var type = "Fire";

            var result = c.Resistances(Difficulty.Merciless, false);

            CheckEquality(new[] { -60f, -60f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Max_Test()
        {
            var c = new Computation();
            var type = "Fire";

            c.Global.Add($"+#% to {type} Resistance", 135f);
            c.Global.Add($"+#% to maximum {type} Resistance", 10f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 85f, 135f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void Chaos_All_Test()
        {
            var c = new Computation();
            var type = "Chaos";

            c.Global.Add($"+#% to All Resistances", 135f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 0f, 0f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void Chaos_Innoc_Test()
        {
            var c = new Computation();
            c.ChaosInoculation = true;
            var type = "Chaos";

            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 100f, 100f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Dual_Test()
        {
            var c = new Computation();
            var type = "Fire";

            c.Global.Add($"+#% to {type} and Lightning Resistances", 10f);
            c.Global.Add($"+#% to Lightning and {type} Resistances", 10f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 20f, 20f }, result[$"{type} Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Shield_Test()
        {
            var c = new Computation();
            c.Global.Add($"+#% Elemental Resistances while holding a Shield", 1f);
            var result = c.Resistances(Difficulty.Normal, true);

            CheckEquality(new[] { 1f, 1f }, result["Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 1f, 1f }, result["Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 1f, 1f }, result["Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, result["Chaos Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Shield_False_Test()
        {
            var c = new Computation();

            c.Global.Add($"+#% Elemental Resistances while holding a Shield", 1f);
            var result = c.Resistances(Difficulty.Normal, false);

            CheckEquality(new[] { 0f, 0f }, result["Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, result["Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, result["Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 0f, 0f }, result["Chaos Resistance: #% (#%)"]);
        }

        [Test]
        public void GetResistance_Full_Test()
        {
            var c = new Computation();

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

            var result = c.Resistances(Difficulty.Normal, true);

            CheckEquality(new[] { 42f, 42f }, result[$"Fire Resistance: #% (#%)"]);
            CheckEquality(new[] { 43f, 43f }, result[$"Cold Resistance: #% (#%)"]);
            CheckEquality(new[] { 76f, 81f }, result[$"Lightning Resistance: #% (#%)"]);
            CheckEquality(new[] { 20f, 20f }, result[$"Chaos Resistance: #% (#%)"]);
        }
    }
}
