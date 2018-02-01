using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Graphs;

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
            var stats = new IStat[] { new StatStub(), new StatStub() };
            var modifier = new Modifier(stats, Form.More, null);
            var graphs = stats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            var sut = CreateSut(s => graphs[s]);

            sut.AddModifier(modifier);

            Mock.Get(graphs[stats[0]]).Verify(g => g.AddModifier(modifier));
            Mock.Get(graphs[stats[1]]).Verify(g => g.AddModifier(modifier));
        }

        [Test]
        public void RemoveModifierCallsStatGraphCorrectly()
        {
            var stats = new IStat[] { new StatStub(), new StatStub(), new StatStub() };
            var modifier = new Modifier(stats, Form.More, null);
            var graphs = stats.ToDictionary(s => s, _ => Mock.Of<IStatGraph>());
            var sut = CreateSut(s => graphs[s]);
            sut.AddModifier(new Modifier(stats.Take(2).ToList(), Form.More, null));

            sut.RemoveModifier(modifier);

            Mock.Get(graphs[stats[0]]).Verify(g => g.RemoveModifier(modifier));
            Mock.Get(graphs[stats[1]]).Verify(g => g.RemoveModifier(modifier));
            Mock.Get(graphs[stats[2]]).Verify(g => g.RemoveModifier(modifier), Times.Never);
        }

        private static CoreCalculationGraph CreateSut(Func<IStat, IStatGraph> graphFactory = null) =>
            new CoreCalculationGraph(graphFactory);
    }
}