using NUnit.Framework;
using POESKillTree.Model;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnitTests.Compute.AttributeHelpers;

namespace UnitTests.Model
{
    [TestFixture]
    public class AttributeSetTest
    {
        [Test]
        public void Add_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            Assert.AreEqual(1, target.Count());
            CheckEquality(1f, target["key"]);
        }

        [Test]
        public void Add_Multiple_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);
            target.Add("key2", 2);

            Assert.AreEqual(2, target.Count());
            CheckEquality(1f, target["key"]);
            CheckEquality(2f, target["key2"]);
        }

        [Test]
        public void Add_Same_Multiple_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);
            target.Add("key", 1);
            target.Add("key", 1);

            Assert.AreEqual(1, target.Count());
            CheckEquality(3f, target["key"]);
        }

        [Test]
        public void Add_ItemMod_Test()
        {
            var target = new AttributeSet();

            var mod = new ItemMod(ItemType.Amulet, "key");
            mod.Value = new[] { 1f }.ToList();
            target.Add(mod);

            Assert.AreEqual(1, target.Count());
            CheckEquality(1f, target["key"]);
        }

        [Test]
        public void Add_ItemMod_ExistingAtt_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            var mod = new ItemMod(ItemType.Amulet, "key");
            mod.Value = new[] { 1f }.ToList();
            target.Add(mod);

            Assert.AreEqual(1, target.Count());
            CheckEquality(2f, target["key"]);
        }

        [Test]
        public void Merge_Test()
        {
            var key = "key";
            var target = new AttributeSet();
            target.Add(key, 1);

            var toMerge = new AttributeSet();
            toMerge.Add(key, 1);

            var merged = target.Merge(toMerge);

            Assert.AreEqual(1, merged.Count());
            CheckEquality(2f, merged[key]);
            CheckEquality(1f, target[key]);
            CheckEquality(1f, toMerge[key]);
        }

        [Test]
        public void Merge_Multiple_Test()
        {
            var key = "key";
            var target = new AttributeSet();
            target.Add(key, 1);
            target.Add(key + "a", 10);

            var toMerge = new AttributeSet();
            toMerge.Add(key, 1);
            target.Add(key + "b", 20);

            var merged = target.Merge(toMerge);

            Assert.AreEqual(3, merged.Count());
            CheckEquality(2f, merged[key]);
            CheckEquality(10f, merged[key + "a"]);
            CheckEquality(20f, merged[key + "b"]);
        }

        [Test]
        public void Remove_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            Assert.AreEqual(1, target.Count());
            CheckEquality(1f, target["key"]);

            target.Remove("key");
            Assert.AreEqual(0, target.Count());
        }

        [Test]
        public void Remove_Multiple_Test()
        {
            var target = new AttributeSet();
            target.Add("key", new[] { 10f, 5, 0 });

            Assert.AreEqual(1, target.Count());

            target.Remove(new KeyValuePair<string, List<float>>("key", new[] { 5f, 5f, 5f }.ToList()));
            Assert.AreEqual(1, target.Count());
            CheckEquality(new[] { 5f, 0f, -5f }, target["key"]);
        }

        [Test]
        public void Remove_Multiple_AllZero_Test()
        {
            var target = new AttributeSet();
            target.Add("key", new[] { 10f, 5, 0 });

            Assert.AreEqual(1, target.Count());

            target.Remove(new KeyValuePair<string, List<float>>("key", target["key"]));
            Assert.AreEqual(0, target.Count());
        }

        [Test]
        public void Matches_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            var matches = target.Matches("k.*");

            Assert.AreEqual(1, matches.Count());
            CheckEquality(1f, matches["key"]);
        }

        [Test]
        public void MatchesAny_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            var matches = target.MatchesAny(new[] { "asdf", "miss", "k.*" });

            Assert.AreEqual(1, matches.Count());
            CheckEquality(1f, matches["key"]);
        }

        [Test]
        public void Add_Replace_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);
            var toReplace = new AttributeSet();
            toReplace.Add("key", 2);
            target.Replace(toReplace);

            Assert.AreEqual(1, target.Count());
            CheckEquality(2f, target["key"]);
        }

        [Test]
        public void Add_GetOrDefault_Test()
        {
            var target = new AttributeSet();
            target.Add("key", 1);

            Assert.AreEqual(1f, target.GetOrDefault("key"));
            Assert.AreEqual(0f, target.GetOrDefault("key", 1));
            Assert.AreEqual(100f, target.GetOrDefault("key", 1, 100f));
        }
    }
}
