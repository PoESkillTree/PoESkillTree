using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace UnitTests
{
	using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    [TestClass]
    public class TestItemDB
    {
        [ClassInitialize]
        public static void Initalize(TestContext testContext)
        {
            AppData.SetApplicationData(Environment.CurrentDirectory);

            if (ItemDB.IsEmpty())
                ItemDB.Load("Data/ItemDB/GemList.xml", true);
        }

        [TestMethod]
        public void TestGems()
        {
#if (PoESkillTree_UseSmallDec_ForAttributes)
			List<SmallDec> expect;
			List<SmallDec> expect2;
			List<SmallDec[]> expectPair;

			// Mixed table and ranges.
			expect = new List<SmallDec> { SmallDec.Zero, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6 };
			for (int level = 1; level < expect.Count; ++level)
				Assert.AreEqual(expect[level], GetValueOf("Molten Strike", "Mana Cost: #", level)[0]);

			// Per level gain.
			expect2 = new List<SmallDec> { SmallDec.Zero, 120, 121.4, 122.8, 124.2, 125.6, 127, 128.4, 129.8, 131.2, 132.6, 134, 135.4, 136.8, 138.2, 139.6, 141, 142.4, 143.8, 145.2, 146.6, 148, 149.4, 150.8, 152.2, 153.6, 155, 156.4, 157.8, 159.2, 160.6 };
			for (int level = 1; level < expect2.Count; ++level)
				Assert.AreEqual(SmallDec.Round(expect2[level], 1), SmallDec.Round(GetValueOf("Molten Strike", "Deals #% of Base Attack Damage", level)[0], 1));

			// Table of damage ranges.
			expectPair = new List<SmallDec[]> { new SmallDec[] { SmallDec.NaN, SmallDec.NaN },
						new SmallDec[] { 7,10 }, new SmallDec[] { 8,11 }, new SmallDec[] { 10,14 },
						new SmallDec[] { 13,20 }, new SmallDec[] { 19,29 }, new SmallDec[] { 29,43 },
						new SmallDec[] { 39,58 }, new SmallDec[] { 52,77 }, new SmallDec[] { 67,101 },
						new SmallDec[] { 87,131 }, new SmallDec[] { 112,168 }, new SmallDec[] { 142,213 },
						new SmallDec[] { 180,270 }, new SmallDec[] { 226,339 }, new SmallDec[] { 283,424 },
						new SmallDec[] { 352,528 }, new SmallDec[] { 437,655 }, new SmallDec[] { 540,810 },
						new SmallDec[] { 632,948 }, new SmallDec[] { 739,1109 }, new SmallDec[] { 819,1229 },
						new SmallDec[] { 908,1362 }, new SmallDec[] { 1005,1508 }, new SmallDec[] { 1113,1669 },
						new SmallDec[] { 1231,1847 }, new SmallDec[] { 1361,2042 }, new SmallDec[] { 1504,2257 },
						new SmallDec[] { 1662,2493 }, new SmallDec[] { 1835,2752 }, new SmallDec[] { 2025,3038 }
			};
#else
            List<float> expect;
            List<double> expect2;
            List<float[]> expectPair;

            // Mixed table and ranges.
            expect = new List<float> { float.NaN, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6};
            for (int level = 1; level < expect.Count; ++level)
                Assert.AreEqual(expect[level], GetValueOf("Molten Strike", "Mana Cost: #", level)[0]);

            // Per level gain.
            expect2 = new List<double> { double.NaN, 120, 121.4, 122.8, 124.2, 125.6, 127, 128.4, 129.8, 131.2, 132.6, 134, 135.4, 136.8, 138.2, 139.6, 141, 142.4, 143.8, 145.2, 146.6, 148, 149.4, 150.8, 152.2, 153.6, 155, 156.4, 157.8, 159.2, 160.6 };
            for (int level = 1; level < expect2.Count; ++level)
                Assert.AreEqual(Math.Round(expect2[level], 1), Math.Round(GetValueOf("Molten Strike", "Deals #% of Base Attack Damage", level)[0], 1));

            // Table of damage ranges.
            expectPair = new List<float[]> { new float[] { float.NaN, float.NaN },
                        new float[] { 7, 10 }, new float[] { 8, 11 }, new float[] { 10, 14 },
                        new float[] { 13, 20 }, new float[] { 19, 29 }, new float[] { 29, 43 },
                        new float[] { 39, 58 }, new float[] { 52, 77 }, new float[] { 67, 101 },
                        new float[] { 87, 131 }, new float[] { 112, 168 }, new float[] { 142, 213 },
                        new float[] { 180, 270 }, new float[] { 226, 339 }, new float[] { 283, 424 },
                        new float[] { 352, 528 }, new float[] { 437, 655 }, new float[] { 540, 810 },
                        new float[] { 632, 948 }, new float[] { 739, 1109 }, new float[] { 819, 1229 },
                        new float[] { 908, 1362 }, new float[] { 1005, 1508 }, new float[] { 1113, 1669 },
                        new float[] { 1231, 1847 }, new float[] { 1361, 2042 }, new float[] { 1504, 2257 },
                        new float[] { 1662, 2493 }, new float[] { 1835, 2752 }, new float[] { 2025, 3038 } 
            };
#endif
			for (int level = 1; level < expectPair.Count; ++level)
            {
                var pair = GetValuePairOf("Fireball", "Deals # to # Fire Damage", level);
                Assert.AreEqual(expectPair[level][0], pair[0]);
                Assert.AreEqual(expectPair[level][1], pair[1]);
            }

            /* Test Fork at Level 5, Quality 10:
             * <Attribute GroupName="#% increased Projectile Damage">
             *   <ValuePerLevel>2</ValuePerLevel>
             *   <ValuePerQuality>0.5</ValuePerQuality>
             * </Attribute>
             */
            Assert.AreEqual(5, GetValueOf("Fork Support", "#% increased Projectile Damage", 5, 10)[0]);

            // Test of "Gems in this item are Supported by level 30 Spell Echo" modifier on Pledge of Hands.
            Assert.AreEqual(10, GetValueOf("Spell Echo Support", "#% less Damage", 30)[0]);
            Assert.AreEqual(80, GetValueOf("Spell Echo Support", "#% more Cast Speed", 30)[0]);
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
		List<SmallDec> GetValueOf(string gemName, string attr, int level, int quality = 0)
		{
			AttributeSet attrs = ItemDB.AttributesOf(gemName, level, quality);

			return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level, quality)[attr] : new List<SmallDec> { SmallDec.NaN };
		}

		List<SmallDec> GetValuePairOf(string gemName, string attr, int level, int quality = 0)
		{
			AttributeSet attrs = ItemDB.AttributesOf(gemName, level, quality);

			return attrs.ContainsKey(attr) ? ItemDB.AttributesOf(gemName, level, quality)[attr] : new List<SmallDec> { SmallDec.NaN, SmallDec.NaN };
		}
#else
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
#endif

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
