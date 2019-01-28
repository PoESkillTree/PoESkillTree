using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;
using static PoESkillTree.Computation.Common.Tests.Helper;
using static PoESkillTree.Computation.Core.Tests.NodeHelper;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class CoreCalculationGraphTest
    {
        [Test]
        public void SutIsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void GetOrAddReturnedInjectedFactoryResult()
        {
            var expected = Mock.Of<IStatGraph>();
            var stat = new StatStub();
            var sut = CreateSut(GraphFactory);
            IStatGraph GraphFactory(IStat s) => s == stat ? expected : throw new AssertionException("Unexpected stat");

            var actual = sut.GetOrAdd(stat);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void GetOrAddAddsToEnumerator()
        {
            var graph = Mock.Of<IStatGraph>();
            var sut = CreateSut(_ => graph);

            sut.GetOrAdd(new StatStub());

            CollectionAssert.Contains(sut, graph);
        }

        [Test]
        public void GetOrAddCachesResult()
        {
            var expected = Mock.Of<IStatGraph>();
            var fail = false;
            var stat = new StatStub();
            var sut = CreateSut(GraphFactory);
            sut.GetOrAdd(stat);
            fail = true;

            var actual = sut.GetOrAdd(stat);

            Assert.AreSame(expected, actual);

            IStatGraph GraphFactory(IStat s) =>
                fail ? throw new AssertionException("Not cached") : expected;
        }

        [Test]
        public void StatGraphsReturnsCorrectResult()
        {
            var graph = Mock.Of<IStatGraph>();
            var stat = new StatStub();
            var sut = CreateSut(_ => graph);
            sut.GetOrAdd(stat);

            var actual = sut.StatGraphs;

            Assert.AreSame(graph, actual[stat]);
        }

        [Test]
        public void RemoveRemovesGraph()
        {
            var graph = Mock.Of<IStatGraph>();
            var stat = new StatStub();
            var sut = CreateSut(_ => graph);
            sut.GetOrAdd(stat);

            sut.Remove(stat);

            CollectionAssert.IsEmpty(sut);
        }

        [Test]
        public void AddModifierCallsStatGraphCorrectly()
        {
            var value = Mock.Of<IValue>();
            var stats = new IStat[] { new StatStub(), new StatStub() };
            var modifier = MockModifier(stats, Form.More, value);
            var graphs = stats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            var path = new PathDefinition(modifier.Source);
            var node = MockDisposableNodeProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value, path) == node);
            var sut = CreateSut(s => graphs[s], nodeFactory);

            sut.AddModifier(modifier);

            Mock.Get(graphs[stats[0]]).Verify(g => g.AddModifier(node, modifier));
            Mock.Get(graphs[stats[1]]).Verify(g => g.AddModifier(node, modifier));
        }

        [Test]
        public void RemoveModifierCallsStatGraphCorrectly()
        {
            var value = Mock.Of<IValue>();
            var stats = new IStat[] { new StatStub(), new StatStub(), new StatStub() };
            var modifier = MockModifier(stats, Form.More, value);
            var graphs = stats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            var path = new PathDefinition(modifier.Source);
            var node = MockDisposableNodeProvider();
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value, path) == node);
            var sut = CreateSut(s => graphs[s], nodeFactory);
            sut.AddModifier(modifier);

            sut.RemoveModifier(modifier);

            Mock.Get(graphs[stats[0]]).Verify(g => g.RemoveModifier(node, modifier));
            Mock.Get(graphs[stats[1]]).Verify(g => g.RemoveModifier(node, modifier));
        }

        [Test]
        public void RemoveModifierDoesNothingIfParameterWasNotAdded()
        {
            var sut = CreateSut();

            sut.RemoveModifier(MockModifier(new StatStub()));
        }

        [Test]
        public void RemoveModifierDisposesNode()
        {
            var value = Mock.Of<IValue>();
            var stats = new IStat[] { new StatStub() };
            var modifier = MockModifier(stats, Form.More, value);
            var graphs = stats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            var path = new PathDefinition(modifier.Source);
            var node = Mock.Of<IDisposableNodeViewProvider>(p => 
                p.DefaultView == MockNode(0) && p.SuspendableView == MockNode(0));
            var nodeFactory = Mock.Of<INodeFactory>(f => f.Create(value, path) == node);
            var sut = CreateSut(s => graphs[s], nodeFactory);
            sut.AddModifier(modifier);

            sut.RemoveModifier(modifier);

            Mock.Get(node).Verify(n => n.Dispose());
        }

        private static CoreCalculationGraph CreateSut(
            Func<IStat, IStatGraph> graphFactory = null, INodeFactory nodeFactory = null) =>
            new CoreCalculationGraph(graphFactory, nodeFactory);
    }
}