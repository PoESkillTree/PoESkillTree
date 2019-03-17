using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    [TestFixture]
    public class ObservableSetTest
    {
        private ObservableSet<string> _set;
        private int _collectionChangedInvocations;
        private int _propertyChangedInvocations;

        [SetUp]
        public void Initialize()
        {
            _set = new ObservableSet<string>
            {
                "item 1",
                "item 2",
                "item 3"
            };
            _set.CollectionChanged += (sender, args) => _collectionChangedInvocations++;
            _set.PropertyChanged += (sender, args) =>
            {
                _propertyChangedInvocations++;
                Assert.AreEqual(nameof(ObservableSet<string>.Count), args.PropertyName);
            };
            _collectionChangedInvocations = 0;
            _propertyChangedInvocations = 0;
        }

        [Test]
        public void TestAdd()
        {
            ExpectCollectionAdd("item");
            _set.Add("item");
            ExpectChangeCalls(1);

            _set.Add("item");
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestClear()
        {
            ExpectCollectionRemove("item 1", "item 2", "item 3");
            _set.Clear();
            ExpectChangeCalls(1);

            _set.Clear();
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestRemove()
        {
            ExpectCollectionRemove("item 2");

            _set.Remove("item");
            ExpectChangeCalls(0);

            _set.Remove("item 2");
            ExpectChangeCalls(1);

            _set.Remove("item 2");
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestReentrancy()
        {
            _set.CollectionChanged += (sender, args) => _set.Add("item");
            Assert.Throws<InvalidOperationException>(() => _set.Remove("item 1"));
        }

        [Test]
        public void TestUnionWith()
        {
            ExpectCollectionAdd("item 4", "item 5");
            _set.UnionWith(new[] { "item 4", "item 3", "item 5" });
            ExpectChangeCalls(1);

            _set.UnionWith(new[] { "item 1", "item 3" });
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestIntersectWith()
        {
            var other = new[] { "item 1", "item 3" };
            var skipNotifierAssertions = false;
            _set.CollectionChanged += (sender, args) =>
            {
                if (skipNotifierAssertions || args.RemovedItems.IsEmpty())
                    return;
                Assert.IsEmpty(args.AddedItems);
                Assert.AreEqual(1, args.RemovedItems.Count);
                Assert.AreEqual("item 2", args.RemovedItems.First());
            };

            // Array
            _set.IntersectWith(other);
            ExpectChangeCalls(1);
            _set.Add("item 2");
            ExpectChangeCalls(1);

            // HashSet
            _set.IntersectWith(new HashSet<string>(other));
            ExpectChangeCalls(1);
            _set.Add("item 2");
            ExpectChangeCalls(1);

            // ObservableSet
            _set.IntersectWith(new ObservableSet<string>(other));
            ExpectChangeCalls(1);
            _set.Add("item 2");
            ExpectChangeCalls(1);

            // No changes
            _set.IntersectWith(new[] { "item 1", "item 3", "item 2" });
            ExpectChangeCalls(0);

            // Empty
            skipNotifierAssertions = true;
            _set.Clear();
            ExpectChangeCalls(1);
            _set.IntersectWith(new[] { "item 1", "item 3" });
            ExpectChangeCalls(0);
        }

        private class CustomEqualityComparaer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return x.Substring(0, x.Length - 1).Equals(y.Substring(0, y.Length - 1));
            }

            public int GetHashCode(string obj)
            {
                return obj.Substring(0, obj.Length - 1).GetHashCode();
            }
        }

        [Test]
        public void TestIntersectWith_CustomEquality()
        {
            // HashSet, non-default equality
            ExpectCollectionRemove("item 2", "item 3");
            _set.IntersectWith(new HashSet<string>(new[] { "item 1", "item 3" }, new CustomEqualityComparaer()));
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestExceptWith()
        {
            ExpectCollectionRemove("item 1", "item 2");
            _set.ExceptWith(new[] { "item 4", "item 2", "item 1" });
            ExpectChangeCalls(1);

            _set.ExceptWith(new[] { "item 1", "item 5" });
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestExceptWith_Empty()
        {
            _set.Clear();
            ExpectChangeCalls(1);
            _set.ExceptWith(new[] { "item 1", "item 4" });
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestSymmetricExceptWith()
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(1, args.AddedItems.Count);
                Assert.AreEqual(1, args.RemovedItems.Count);
                Assert.AreEqual("item 4", args.AddedItems.First());
                Assert.AreEqual("item 2", args.RemovedItems.First());
            };
            _set.SymmetricExceptWith(new[] { "item 2", "item 4" });
            ExpectChangeCalls(1, 0);

            _set.SymmetricExceptWith(new string[0]);
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestSymmetricExceptWith_Empty()
        {
            _set.Clear();
            ExpectChangeCalls(1);
            ExpectCollectionAdd("item 1", "item 4");
            _set.SymmetricExceptWith(new[] { "item 1", "item 4" });
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestExceptAndUnionWith()
        {
            _set.CollectionChanged += (sender, args) =>
            {
                CollectionAssert.AreEquivalent(new[] { "item 2" }, args.RemovedItems);
                CollectionAssert.AreEquivalent(new[] { "item 4", "item 5" }, args.AddedItems);
            };

            _set.ExceptAndUnionWith(
                new[] { "item 2", "item 3", "item 4" },
                new[] { "item 1", "item 3", "item 4", "item 5" });

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 3", "item 4", "item 5" }, _set);
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestRemoveAndAdd_RemovedAndAdded()
        {
            _set.CollectionChanged += (sender, args) =>
            {
                CollectionAssert.AreEquivalent(new[] { "item 3" }, args.RemovedItems);
                CollectionAssert.AreEquivalent(new[] { "item 4" }, args.AddedItems);
            };

            _set.RemoveAndAdd("item 3", "item 4");

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 2", "item 4" }, _set);
            ExpectChangeCalls(1, 0);
        }

        [Test]
        public void TestRemoveAndAdd_RemovedAndNotAdded()
        {
            ExpectCollectionRemove("item 3");

            _set.RemoveAndAdd("item 3", "item 1");

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 2" }, _set);
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestRemoveAndAdd_NotRemovedAndAdded()
        {
            ExpectCollectionAdd("item 4");

            _set.RemoveAndAdd("item 5", "item 4");

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 2", "item 3", "item 4" }, _set);
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestRemoveAndAdd_NotRemovedAndNotAdded()
        {
            _set.RemoveAndAdd("item 4", "item 3");

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 2", "item 3" }, _set);
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestRemoveAndAdd_SameRemovedAndAddedAndPreviouslyContained()
        {
            _set.RemoveAndAdd("item 3", "item 3");

            CollectionAssert.AreEquivalent(new[] { "item 1", "item 2", "item 3" }, _set);
            ExpectChangeCalls(0);
        }

        private void ExpectCollectionAdd(params string[] items)
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.IsEmpty(args.RemovedItems);
                Assert.AreEqual(items.Length, args.AddedItems.Count);
                CollectionAssert.AreEquivalent(items, args.AddedItems);
            };
        }

        private void ExpectCollectionRemove(params string[] items)
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.IsEmpty(args.AddedItems);
                Assert.AreEqual(items.Length, args.RemovedItems.Count);
                CollectionAssert.AreEquivalent(items, args.RemovedItems);
            };
        }

        private void ExpectChangeCalls(int count)
            => ExpectChangeCalls(count, count);

        private void ExpectChangeCalls(int collectionChangedCount, int propertyChangedCount)
        {
            Assert.AreEqual(collectionChangedCount, _collectionChangedInvocations);
            Assert.AreEqual(propertyChangedCount, _propertyChangedInvocations);
            _collectionChangedInvocations = 0;
            _propertyChangedInvocations = 0;
        }
    }
}