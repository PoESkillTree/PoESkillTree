using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;

namespace PoESkillTree.Tests.Computation.Model
{
    [TestFixture]
    public class ObservableCalculatorTest
    {
        [Test]
        public void ObserveNodeGeneratesCorrectValues()
        {
            var expected = new[] { (NodeValue?) 0, (NodeValue?) 0, null, (NodeValue?) 1 };
            var stat = new Stat("");
            var index = 0;
            var nodeMock = new Mock<ICalculationNode>();
            nodeMock.SetupGet(n => n.Value).Returns(() => expected[index++]);
            var calculator = Mock.Of<ICalculator>(c =>
                c.NodeRepository.GetNode(stat, NodeType.Total, PathDefinition.MainPath) == nodeMock.Object);
            var sut = CreateSut(calculator);

            var actual = new List<NodeValue?>();
            using (sut.ObserveNode(stat).Subscribe(actual.Add))
            {
                nodeMock.Raise(n => n.ValueChanged += null, nodeMock.Object, EventArgs.Empty);
                nodeMock.Raise(n => n.ValueChanged += null, nodeMock.Object, EventArgs.Empty);
                nodeMock.Raise(n => n.ValueChanged += null, nodeMock.Object, EventArgs.Empty);
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ObserveExplicitlyRegisteredStatsGeneratesCorrectValues()
        {
            var elements = Enumerable.Range(0, 3)
                .Select(i => (Mock.Of<ICalculationNode>(), (IStat) new Stat(i.ToString())))
                .ToList();
            var expected = new[]
            {
                new CollectionChangeEventArgs(CollectionChangeAction.Add, elements[0]),
                new CollectionChangeEventArgs(CollectionChangeAction.Refresh, elements),
                new CollectionChangeEventArgs(CollectionChangeAction.Remove, elements[1]),
            };
            var nodeCollectionMock = new Mock<INodeCollection<IStat>>();
            nodeCollectionMock.Setup(c => c.GetEnumerator()).Returns(() => elements.GetEnumerator());
            var calculator = Mock.Of<ICalculator>(c =>
                c.ExplicitlyRegisteredStats == nodeCollectionMock.Object);
            var sut = CreateSut(calculator);

            var actual = new List<CollectionChangeEventArgs>();
            using (sut.ObserveExplicitlyRegisteredStats().Subscribe(actual.Add))
            {
                foreach (var args in expected)
                {
                    nodeCollectionMock.Raise(n => n.CollectionChanged += null, nodeCollectionMock.Object, args);
                }
            }

            Assert.AreEqual(3, actual.Count);
            Assert.AreEqual(expected.Select(e => e.Action), actual.Select(e => e.Action));
            Assert.AreEqual(expected[0].Element, actual[0].Element);
            Assert.AreEqual(expected[1].Element, actual[1].Element);
            Assert.AreEqual(expected[2].Element, actual[2].Element);
        }

        [Test]
        public async Task ForEachUpdateCalculatorAsyncCallsCalculatorCorrectly()
        {
            var expected = new[]
            {
                new CalculatorUpdate(
                    new[] { new Modifier(new[] { new Stat("") }, default, null, null), },
                    new Modifier[0]),
                CalculatorUpdate.Empty,
            };
            var observable = expected.ToObservable();
            var calculatorMock = new Mock<ICalculator>();
            var sut = CreateSut(calculatorMock.Object);

            await sut.ForEachUpdateCalculatorAsync(observable);

            foreach (var update in expected)
            {
                calculatorMock.Verify(c => c.Update(update));
            }
        }

        private static ObservableCalculator CreateSut(ICalculator calculator)
        {
            var scheduler = ImmediateScheduler.Instance;
            return new ObservableCalculator(calculator, scheduler);
        }
    }
}