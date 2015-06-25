using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace UnitTests
{
    [TestClass]
    public class TestItemDB
    {
        [ClassInitialize]
        public static void Initalize(TestContext testContext)
        {
            AppData.SetApplicationData(Environment.CurrentDirectory);

            if (ItemDB.IsEmpty())
                ItemDB.Load("Items.xml", true);
        }

        [TestMethod]
        public void TestGems()
        {
            List<float> expect;
            List<float[]> expectPair;

            // Mixed table and ranges.
            expect = new List<float> { float.NaN, 6, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 10, 10, 10, 10 };
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf("Molten Strike", "Mana Cost: #", level)[0]);

            // Per level gain.
            expect = new List<float> { float.NaN, float.NaN, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 44, 48, 52, 56, 60, 64, 68, 72, 76, 80 };
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf("Molten Strike", "#% increased Physical Damage", level)[0]);

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
                var pair = GetValuePairOf("Fireball", "Deals #-# Fire Damage", level);
                Assert.AreEqual(expectPair[level][0], pair[0]);
                Assert.AreEqual(expectPair[level][1], pair[1]);
            }

            /* Test Fork at Level 5, Quality 10:
             * <Attribute GroupName="#% increased Projectile Damage">
             *   <ValuePerLevel>2</ValuePerLevel>
             *   <ValuePerQuality>0.5</ValuePerQuality>
             * </Attribute>
             */
            Assert.AreEqual(13, GetValueOf("Fork", "#% increased Projectile Damage", 5, 10)[0]);

            // Test of "Gems in this item are Supported by level 30 Spell Echo" modifier on Pledge of Hands.
            Assert.AreEqual(10, GetValueOf("Spell Echo", "#% less Damage", 30)[0]);
            Assert.AreEqual(79, GetValueOf("Spell Echo", "#% more Cast Speed", 30)[0]);
        }

        List<float> GetValueOf(string gemName, string attr, int level, int quality = 0)
        {
            AttributeSet attrs = ItemDB.AttributesOf(gemName, level, quality);

            return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level, quality)[attr] : new List<float> { float.NaN };
        }

        List<float> GetValuePairOf(string gemName, string attr, int level, int quality = 0)
        {
            AttributeSet attrs = ItemDB.AttributesOf(gemName, level, quality);

            return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level, quality)[attr] : new List<float> { float.NaN, float.NaN };
        }

        [TestMethod]
        public void TestMergeDB()
        {
            ItemDB.Merge("TestItems.xml");
            ItemDB.Index();

            Assert.AreEqual(5, ItemDB.AttributesOf("TestGem", 5, 0)["Attribute1: #"][0]);
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 10, 0)["Attribute1: #"][0]);

            ItemDB.Merge("TestMerge.xml");
            ItemDB.Index();

            // Attribute1: <Value Level="5">21</Value> <ValuePerLevel>5</ValuePerLevel>
            Assert.AreEqual(21, ItemDB.AttributesOf("TestGem", 5, 0)["Attribute1: #"][0], "Level 5");
            Assert.AreEqual(45, ItemDB.AttributesOf("TestGem", 10, 0)["Attribute1: #"][0], "Level 10");
            // Attribute2: <Value Level="5">5</Value> <Value Level="6">7</Value> <ValueForLevel From="1" To="10">10</ValueForLevel>
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute2: #"][0], "Level 1");
            Assert.AreEqual(6, ItemDB.AttributesOf("TestGem", 5, 0)["Attribute2: #"][0], "Level 5");
            Assert.AreEqual(7, ItemDB.AttributesOf("TestGem", 6, 0)["Attribute2: #"][0], "Level 6");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 10, 0)["Attribute2: #"][0], "Level 10");
            // Attribute3: <Value Level="5">5</Value> <ValueForLevel From="1" To="7">10</ValueForLevel> <ValueForLevel From="8" To="15">15</ValueForLevel>
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute3: #"][0], "Level 1");
            Assert.AreEqual(5, ItemDB.AttributesOf("TestGem", 5, 0)["Attribute3: #"][0], "Level 5");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 7, 0)["Attribute3: #"][0], "Level 7");
            Assert.AreEqual(15, ItemDB.AttributesOf("TestGem", 8, 0)["Attribute3: #"][0], "Level 8");
            Assert.AreEqual(15, ItemDB.AttributesOf("TestGem", 15, 0)["Attribute3: #"][0], "Level 15");
            // Attribute4: <Value Level="5">5</Value> <ValueForLevel From="1" To="7">10</ValueForLevel> <ValueForLevel From="8" To="9">15</ValueForLevel> <Value Level="10">10</Value>
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute4: #"][0], "Level 1");
            Assert.AreEqual(5, ItemDB.AttributesOf("TestGem", 5, 0)["Attribute4: #"][0], "Level 5");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 7, 0)["Attribute4: #"][0], "Level 7");
            Assert.AreEqual(15, ItemDB.AttributesOf("TestGem", 8, 0)["Attribute4: #"][0], "Level 8");
            Assert.AreEqual(15, ItemDB.AttributesOf("TestGem", 9, 0)["Attribute4: #"][0], "Level 9");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 10, 0)["Attribute4: #"][0], "Level 10");
            // Attribute5: <Value Level="1">1</Value> <Value Level="2">2</Value> <Value Level="3">3</Value>
            Assert.AreEqual(1, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute5: #"][0], "Level 1");
            Assert.AreEqual(2, ItemDB.AttributesOf("TestGem", 2, 0)["Attribute5: #"][0], "Level 2");
            Assert.AreEqual(3, ItemDB.AttributesOf("TestGem", 3, 0)["Attribute5: #"][0], "Level 3");
            // Attribute6: <Value Level="1">1</Value> <Value Level="2">2</Value> ... <Value Level="30">30</Value>
            Assert.AreEqual(1, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute6: #"][0], "Level 1");
            Assert.AreEqual(2, ItemDB.AttributesOf("TestGem", 2, 0)["Attribute6: #"][0], "Level 2");
            Assert.AreEqual(30, ItemDB.AttributesOf("TestGem", 30, 0)["Attribute6: #"][0], "Level 30");
            // Attribute7: <Value Level="1">1</Value> <Value Level="2">2</Value> ... <Value Level="30">30</Value>
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute7: #"][0], "Level 1");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 2, 0)["Attribute7: #"][0], "Level 2");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 30, 0)["Attribute7: #"][0], "Level 30");
            // Attribute8: <Value>10</Value>
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 1, 0)["Attribute8: #"][0], "Level 1");
            Assert.AreEqual(10, ItemDB.AttributesOf("TestGem", 30, 0)["Attribute8: #"][0], "Level 30");
        }
    }
}
