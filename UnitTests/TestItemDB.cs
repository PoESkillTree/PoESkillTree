using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Model;
using System.Text.RegularExpressions;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class TestItemDB
    {
        [ClassInitialize]
        public static void Initalize(TestContext testContext)
        {
            if (ItemDB.IsEmpty())
                ItemDB.Initialize(@"..\..\..\WPFSkillTree");
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            ItemDB.Clear();
        }

        [TestMethod]
        public void TestGems()
        {
            List<float> expect;
            List<float[]> expectPair;

            // Mixed table and ranges.
            expect = new List<float> { float.NaN, 6, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 10, 10, 10, 10 };
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf("Molten Strike", level, "Mana Cost: #")[0]);

            // Per level gain.
            expect = new List<float> { float.NaN, float.NaN, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80 };
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf("Molten Strike", level, "#% increased Physical Damage")[0]);

            // Table of damage ranges.
            expectPair = new List<float[]> { new float[] { float.NaN, float.NaN },
                new float[] { 5, 10 }, new float[] { 7, 11 }, new float[] { 9, 14 }, new float[] { 13, 19 }, new float[] { 17, 25 },
                new float[] { 23, 34 }, new float[] { 32, 48 }, new float[] { 44, 67 }, new float[] { 63, 95 }, new float[] { 89, 133 },
                new float[] { 110, 165 }, new float[] { 135, 203 }, new float[] { 157, 236 }, new float[] { 183, 274 }, new float[] { 212, 318 },
                new float[] { 245, 368 }, new float[] { 283, 425 }, new float[] { 326, 489 }, new float[] { 358, 537 }, new float[] { 393, 590 },
                new float[] { 431, 647 }, new float[] { 472, 709 }, new float[] { 518, 776 }, new float[] { 567, 850 }, new float[] { 620, 930 },
                new float[] { 678, 1017 }, new float[] { 741, 1111 }, new float[] { 809, 1214 }, new float[] { 884, 1326 }, new float[] { 965, 1447 }
            };
            for (int level = 1; level < expectPair.Count; ++level)
            {
                var pair = GetValuePairOf("Fireball", level, "Deals #-# Fire Damage");
                Assert.AreEqual(expectPair[level][0], pair[0]);
                Assert.AreEqual(expectPair[level][1], pair[1]);
            }
        }

        List<float> GetValueOf(string gemName, int level, string attr)
        {
            AttributeSet attrs = ItemDB.AttributesOf(gemName, level);

            return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level)[attr] : new List<float> { float.NaN };
        }

        List<float> GetValuePairOf(string gemName, int level, string attr)
        {
            AttributeSet attrs = ItemDB.AttributesOf(gemName, level);

            return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level)[attr] : new List<float> { float.NaN, float.NaN };
        }
    }
}
