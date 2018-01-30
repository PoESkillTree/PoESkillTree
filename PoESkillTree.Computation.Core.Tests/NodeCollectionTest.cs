using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests
{
    [TestFixture]
    public class NodeCollectionTest
    {
        [Test]
        public void SutIsINodeCollection()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<INodeCollection<int>>(sut);
        }

        [Test]
        public void IsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void NodePropertiesIsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut.NodeProperties);
        }

        [Test]
        public void AddAddsNode()
        {
            var node = MockNode();
            var sut = CreateSut();

            sut.Add(node, 0);

            CollectionAssert.AreEquivalent(new[] { node }, sut);
        }

        [Test]
        public void RemoveRemovesNode()
        {
            var node = MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);

            sut.Remove(node);

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void IsSet()
        {
            var node = MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);
            sut.Add(node, 0);

            CollectionAssert.AllItemsAreUnique(sut);
        }

        [Test]
        public void CountReturnsCorrectResult()
        {
            var sut = CreateSut();
            sut.Add(MockNode(), 0);
            sut.Add(MockNode(), 0);
            sut.Add(MockNode(), 0);

            Assert.AreEqual(3, sut.Count);
        }

        [Test]
        public void AddRaisesCollectionChanged()
        {
            var node = MockNode();
            var sut = CreateSut();
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreSame(sender, sut);
                Assert.AreEqual(NodeCollectionChangeAction.Add, args.Action);
                Assert.AreEqual(node, args.Element);
                raised = true;
            };

            sut.Add(node, 0);

            Assert.IsTrue(raised);
        }

        [Test]
        public void RemoveRaisesCollectionChanged()
        {
            var node = MockNode();
            var sut = CreateSut();
            sut.Add(node, 0);
            var raised = false;
            sut.CollectionChanged += (sender, args) =>
            {
                Assert.AreSame(sender, sut);
                Assert.AreEqual(NodeCollectionChangeAction.Remove, args.Action);
                Assert.AreEqual(node, args.Element);
                raised = true;
            };

            sut.Remove(node);

            Assert.IsTrue(raised);
        }

        [Test]
        public void RemoveDoesNotRaiseCollectionChangeIfNodeWasNotAdded()
        {
            var node = MockNode();
            var sut = CreateSut();

            sut.CollectionChanged += (sender, args) => Assert.Fail();
            sut.Remove(node);
        }

        [Test]
        public void NodePropertiesIsCorrect()
        {
            var removed = MockNode();
            var expected = new[]
            {
                (MockNode(), 0),
                (MockNode(), 1),
                (MockNode(), 2),
            }.Select(t => new KeyValuePair<ICalculationNode, int>(t.Item1, t.Item2)).ToList();
            var sut = CreateSut();

            sut.Add(removed, -1);
            expected.ForEach(t => sut.Add(t.Key, t.Value));
            sut.Remove(removed);

            CollectionAssert.AreEquivalent(expected, sut.NodeProperties);
        }

        [TestCase(3)]
        [TestCase(0)]
        public void SubscriberCountReturnsCorrectResult(int expected)
        {
            var sut = CreateSut();
            Enumerable.Repeat(0, expected).ForEach(_ => sut.CollectionChanged += (sender, args) => { });

            var actual = sut.SubscriberCount;

            Assert.AreEqual(expected, actual);
        }

        private static NodeCollection<int> CreateSut() =>
            new NodeCollection<int>();
    }
}