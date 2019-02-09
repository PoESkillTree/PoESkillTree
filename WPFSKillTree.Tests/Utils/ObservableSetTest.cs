using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using POESKillTree.Utils;

namespace PoESkillTree.Tests.Utils
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
            ExpectCollectionChange(NotifyCollectionChangedAction.Add, "item");
            _set.Add("item");
            ExpectChangeCalls(1);

            _set.Add("item");
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestClear()
        {
            ExpectCollectionChange(NotifyCollectionChangedAction.Remove, "item 1", "item 2", "item 3");
            _set.Clear();
            ExpectChangeCalls(1);

            _set.Clear();
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestRemove()
        {
            ExpectCollectionChange(NotifyCollectionChangedAction.Remove, "item 2");

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
        public void TestRangeChangeUnsupported()
        {
            var first = true;
            _set.CollectionChanged += (sender, args) =>
            {
                if (first)
                {
                    first = false;
                    Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                    throw new NotSupportedException();
                }
                Assert.AreEqual(NotifyCollectionChangedAction.Reset, args.Action);
            };
            _set.Clear();
            _set.UnionWith(new[] {"item 4", "item 5"});
            Assert.AreEqual(3, _collectionChangedInvocations);
            Assert.AreEqual(2, _propertyChangedInvocations);
        }

        [Test]
        public void TestUnionWith()
        {
            ExpectCollectionChange(NotifyCollectionChangedAction.Add, "item 4", "item 5");
            _set.UnionWith(new[] { "item 4", "item 3", "item 5" });
            ExpectChangeCalls(1);

            _set.UnionWith(new[] { "item 1", "item 3" });
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestIntersectWith()
        {
            var other = new[] {"item 1", "item 3"};
            var skipNotifierAssertions = false;
            _set.CollectionChanged += (sender, args) =>
            {
                if (skipNotifierAssertions || args.Action == NotifyCollectionChangedAction.Add)
                    return;
                Assert.AreEqual(NotifyCollectionChangedAction.Remove, args.Action);
                Assert.AreEqual(1, args.OldItems.Count);
                Assert.AreEqual("item 2", args.OldItems[0]);
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
            ExpectCollectionChange(NotifyCollectionChangedAction.Remove, "item 2", "item 3");
            _set.IntersectWith(new HashSet<string>(new[] {"item 1", "item 3"}, new CustomEqualityComparaer()));
            ExpectChangeCalls(1);
        }

        [Test]
        public void TestExceptWith()
        {
            ExpectCollectionChange(NotifyCollectionChangedAction.Remove, "item 1", "item 2");
            _set.ExceptWith(new[] {"item 4", "item 2", "item 1"});
            ExpectChangeCalls(1);

            _set.ExceptWith(new[] {"item 1", "item 5"});
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestExceptWith_Empty()
        {
            _set.Clear();
            ExpectChangeCalls(1);
            _set.ExceptWith(new[] {"item 1", "item 4"});
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestSymmetricExceptWith()
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(NotifyCollectionChangedAction.Replace, args.Action);
                Assert.AreEqual(1, args.NewItems.Count);
                Assert.AreEqual(1, args.OldItems.Count);
                Assert.AreEqual("item 4", args.NewItems[0]);
                Assert.AreEqual("item 2", args.OldItems[0]);
            };
            _set.SymmetricExceptWith(new[] {"item 2", "item 4"});
            ExpectChangeCalls(1);

            _set.SymmetricExceptWith(new string[0]);
            ExpectChangeCalls(0);
        }

        [Test]
        public void TestSymmetricExceptWith_Empty()
        {
            _set.Clear();
            ExpectChangeCalls(1);
            ExpectCollectionChange(NotifyCollectionChangedAction.Add, "item 1", "item 4");
            _set.SymmetricExceptWith(new[] { "item 1", "item 4" });
            ExpectChangeCalls(1);
        }

        private void ExpectCollectionChange(NotifyCollectionChangedAction action, string item)
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(action, args.Action);
                var items = action == NotifyCollectionChangedAction.Add ? args.NewItems : args.OldItems;
                Assert.AreEqual(1, items.Count);
                Assert.AreEqual(item, items[0]);
            };
        }

        private void ExpectCollectionChange(NotifyCollectionChangedAction action, params string[] items)
        {
            _set.CollectionChanged += (sender, args) =>
            {
                Assert.AreEqual(action, args.Action);
                var coll = action == NotifyCollectionChangedAction.Add ? args.NewItems : args.OldItems;
                Assert.AreEqual(items.Length, coll.Count);
                CollectionAssert.AreEquivalent(items, coll);
            };
        }

        private void ExpectChangeCalls(int count)
        {
            Assert.AreEqual(count, _collectionChangedInvocations);
            Assert.AreEqual(count, _propertyChangedInvocations);
            _collectionChangedInvocations = 0;
            _propertyChangedInvocations = 0;
        }
    }
}
