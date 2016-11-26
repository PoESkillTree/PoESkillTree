using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace UnitTests
{
    [TestClass]
    public class TestItemDB
    {


        [TestMethod]
        public void TestGems()
        {
            var DB = GemDB.LoadFromText(FileEx.GetResource<GemDB>("POESKillTree.Data.ItemDB.GemList.xml"), true);
            List<float> expect;
            List<double> expect2;
            List<float[]> expectPair;

            // Mixed table and ranges.
            expect = new List<float> { float.NaN, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6};
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf(DB, "Molten Strike", "Mana Cost: #", level)[0]);

            // Per level gain.
            expect2 = new List<double> { double.NaN, 120, 121.4, 122.8, 124.2, 125.6, 127, 128.4, 129.8, 131.2, 132.6, 134, 135.4, 136.8, 138.2, 139.6, 141, 142.4, 143.8, 145.2, 146.6, 148, 149.4, 150.8, 152.2, 153.6, 155, 156.4, 157.8, 159.2, 160.6 };
            for (int level = 1; level < expect2.Count; ++level)
                Assert.AreEqual(Math.Round(expect2[level], 1), Math.Round(GetValueOf(DB, "Molten Strike", "Deals #% of Base Attack Damage", level)[0], 1));

            // Table of damage ranges.
            expectPair = new List<float[]> { new float[] { float.NaN, float.NaN },
                        new float[] { 7, 10 }, new float[] { 8, 11 }, new float[] { 9, 14 }, 
                        new float[] { 13, 19 }, new float[] { 18, 27 }, new float[] { 26, 40 }, 
                        new float[] { 35, 52 }, new float[] { 45, 68 }, new float[] { 58, 86 }, 
                        new float[] { 73, 109 }, new float[] { 91, 137 }, new float[] { 113, 170 }, 
                        new float[] { 140, 210 }, new float[] { 172, 258 }, new float[] { 210, 315 }, 
                        new float[] { 256, 384 }, new float[] { 310, 466 }, new float[] { 375, 563 }, 
                        new float[] { 432, 647 }, new float[] { 496, 744 }, new float[] { 543, 815 }, 
                        new float[] { 595, 893 }, new float[] { 651, 977 }, new float[] { 713, 1069 }, 
                        new float[] { 779, 1169 }, new float[] { 852, 1278 }, new float[] { 931, 1396 }, 
                        new float[] { 1016, 1524 }, new float[] { 1109, 1664 }, new float[] { 1210, 1815 }
            };
            for (int level = 1; level < expectPair.Count; ++level)
            {
                var pair = GetValuePairOf(DB, "Fireball", "Deals # to # Fire Damage", level);
                Assert.AreEqual(expectPair[level][0], pair[0]);
                Assert.AreEqual(expectPair[level][1], pair[1]);
            }

            /* Test Fork at Level 5, Quality 10:
             * <Attribute GroupName="#% increased Projectile Damage">
             *   <ValuePerLevel>2</ValuePerLevel>
             *   <ValuePerQuality>0.5</ValuePerQuality>
             * </Attribute>
             */
            Assert.AreEqual(5, GetValueOf(DB, "Fork Support", "#% increased Projectile Damage", 5, 10)[0]);

            // Test of "Gems in this item are Supported by level 30 Spell Echo" modifier on Pledge of Hands.
            Assert.AreEqual(10, GetValueOf(DB, "Spell Echo Support", "#% less Damage", 30)[0]);
            Assert.AreEqual(80, GetValueOf(DB, "Spell Echo Support", "#% more Cast Speed", 30)[0]);
        }

        List<float> GetValueOf(GemDB DB, string gemName, string attr, int level, int quality = 0)
        {
            AttributeSet attrs = DB.AttributesOf(gemName, level, quality);

            return attrs.ContainsKey(attr) ? DB.AttributesOf(gemName, level, quality)[attr] : new List<float> { float.NaN };
        }

        List<float> GetValuePairOf(GemDB DB, string gemName, string attr, int level, int quality = 0)
        {
            AttributeSet attrs = DB.AttributesOf(gemName, level, quality);

            return attrs.ContainsKey(attr) ? DB.AttributesOf(gemName, level, quality)[attr] : new List<float> { float.NaN, float.NaN };
        }

        [TestMethod]
        public void TestMergeDB()
        {
            var target = GemDB.LoadFromText(FileEx.GetResource<GemDB>("POESKillTree.Data.ItemDB.GemList.xml"));
            var merge = GemDB.LoadFromText(FileEx.GetResource<TestItemDB>("UnitTests.TestItems.xml"));

            target.Merge(merge);
            target.Index();

            // Attribute1: <Value Level="5">21</Value> <ValuePerLevel>5</ValuePerLevel>
            Assert.AreEqual(21, target.AttributesOf("TestGem", 5, 0)["Attribute1: #"][0], "Level 5");
            Assert.AreEqual(45, target.AttributesOf("TestGem", 10, 0)["Attribute1: #"][0], "Level 10");
            // Attribute2: <Value Level="5">5</Value> <Value Level="6">7</Value> <ValueForLevel From="1" To="10">10</ValueForLevel>
            Assert.AreEqual(10, target.AttributesOf("TestGem", 1, 0)["Attribute2: #"][0], "Level 1");
            Assert.AreEqual(5, target.AttributesOf("TestGem", 5, 0)["Attribute2: #"][0], "Level 5");
        }
    }
}
