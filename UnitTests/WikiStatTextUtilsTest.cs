using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static UpdateDB.DataLoading.WikiStatTextUtils;

namespace UnitTests
{
    [TestClass]
    public class WikiStatTextUtilsTest
    {

        [TestMethod]
        public void ConvertStatText_MultipleStats_SplitsCorrectly()
        {
            const string text = "+(60-80) to Intelligence<br>" +
                                "+(60-75) to maximum Life<br>" +
                                "(200-220)% increased Armour<br>" +
                                "(25-35)% increased Global Critical Strike Chance<br>" +
                                "Leech applies instantly on Critical Strike";

            var stats = ConvertStatText(text).Select(s => s.Name).ToList();

            Assert.AreEqual(5, stats.Count);
            Assert.AreEqual("+# to Intelligence", stats[0]);
            Assert.AreEqual("+# to maximum Life", stats[1]);
            Assert.AreEqual("#% increased Armour", stats[2]);
            Assert.AreEqual("#% increased Global Critical Strike Chance", stats[3]);
            Assert.AreEqual("Leech applies instantly on Critical Strike", stats[4]);
        }

        [TestMethod]
        public void ConvertStatText_SimpleLink_ReplacesLink()
        {
            const string text = "+(60-80) to [[Intelligence]]";

            var stats = ConvertStatText(text).Select(s => s.Name).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("+# to Intelligence", stats[0]);
        }

        [TestMethod]
        public void ConvertStatText_ComplexLink_ReplacesLink()
        {
            const string text = "15% increased Quantity of Items Dropped by Slain [[Freeze|Frozen]] Enemies";

            var stats = ConvertStatText(text).Select(s => s.Name).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("#% increased Quantity of Items Dropped by Slain Frozen Enemies", stats[0]);
        }

        [TestMethod]
        public void ConvertStatText_NoValues()
        {
            const string text = "Leech applies instantly on Critical Strike";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("Leech applies instantly on Critical Strike", stats[0].Name);
            Assert.AreEqual("", stats[0].FromAsString);
            Assert.AreEqual("", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_FixedValue()
        {
            const string text = "+60 to Intelligence";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("+# to Intelligence", stats[0].Name);
            Assert.AreEqual("60", stats[0].FromAsString);
            Assert.AreEqual("60", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_Range()
        {
            const string text = "+(60-80) to Intelligence";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("+# to Intelligence", stats[0].Name);
            Assert.AreEqual("60", stats[0].FromAsString);
            Assert.AreEqual("80", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_NegativeRange()
        {
            const string text = "+(-25-50)% to Fire Resistance";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("+#% to Fire Resistance", stats[0].Name);
            Assert.AreEqual("-25", stats[0].FromAsString);
            Assert.AreEqual("50", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_AddedNoRanged()
        {
            const string text = "Adds 1 to 5 Lightning Damage";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("Adds # to # Lightning Damage", stats[0].Name);
            Assert.AreEqual("1 5", stats[0].FromAsString);
            Assert.AreEqual("1 5", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_AddedOneRanged()
        {
            const string text = "Adds 1 to (80-100) Lightning Damage";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("Adds # to # Lightning Damage", stats[0].Name);
            Assert.AreEqual("1 80", stats[0].FromAsString);
            Assert.AreEqual("1 100", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_AddedBothRanged()
        {
            const string text = "Adds (32-40) to (48-60) Cold Damage";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("Adds # to # Cold Damage", stats[0].Name);
            Assert.AreEqual("32 48", stats[0].FromAsString);
            Assert.AreEqual("40 60", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_MultipleFixedValues1()
        {
            const string text = "With 5 Corrupted Items Equipped: Gain Soul Eater for 10 seconds on Vaal Skill use";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("With # Corrupted Items Equipped: Gain Soul Eater for # seconds on Vaal Skill use", stats[0].Name);
            Assert.AreEqual("5 10", stats[0].FromAsString);
            Assert.AreEqual("5 10", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_MultipleFixedValues2()
        {
            const string text = "Adds 1 to 3 Physical Damage to Attacks per 25 Dexterity";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("Adds # to # Physical Damage to Attacks per # Dexterity", stats[0].Name);
            Assert.AreEqual("1 3 25", stats[0].FromAsString);
            Assert.AreEqual("1 3 25", stats[0].ToAsString);
        }

        [TestMethod]
        public void ConvertStatText_Corrupted_IsSkipped()
        {
            const string text = "<em class=\"tc -corrupted\">Corrupted</em>";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(0, stats.Count);
        }

        [TestMethod]
        public void ConvertStatText_InvalidStat_IsSkipped()
        {
            const string text = "<small>[''[[#Modifiers|see notes]]'']</small>";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(0, stats.Count);
        }

        [TestMethod]
        public void ConvertStatText_HtmlEncoding_IsDecoded()
        {
            const string text = "&#60;Random Keystone&#62;";

            var stats = ConvertStatText(text).ToList();

            Assert.AreEqual(1, stats.Count);
            Assert.AreEqual("<Random Keystone>", stats[0].Name);
        }
    }
}