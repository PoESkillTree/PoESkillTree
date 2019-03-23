using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.NodeCollections;
using static PoESkillTree.Computation.Common.ExplicitRegistrationTypes;
using static PoESkillTree.Computation.Core.Graphs.NodeSelectorHelper;

namespace PoESkillTree.Computation.Core.Graphs
{
    [TestFixture]
    public class UserSpecifiedValuePruningRuleSetTest
    {
        [Test]
        public void SelectStatsConsideredForRemovalReturnsUserSpecifiedValueStats()
        {
            var stats = new IStat[]
            {
                new StatStub
                {
                    ExplicitRegistrationType = GainOnAction(null, "", default)
                },
                new StatStub
                {
                    ExplicitRegistrationType = UserSpecifiedValue(true)
                },
                new StatStub(),
            };
            var expected = new[] { stats[1] };
            var sut = CreateSut();

            var actual = stats.Where(s => sut.CanStatBeConsideredForRemoval(s, null));

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SelectRemovableNodesByNodeTypeReturnsDefaultRuleSetResult()
        {
            var statGraph = Mock.Of<IReadOnlyStatGraph>();
            var expected = new[] { Selector(NodeType.Total) };
            var defaultRuleSet = Mock.Of<IGraphPruningRuleSet>(r =>
                r.SelectRemovableNodesByNodeType(statGraph) == expected);
            var sut = CreateSut(defaultRuleSet);

            var actual = sut.SelectRemovableNodesByNodeType(statGraph);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SelectRemovableNodesByFormReturnsDefaultRuleSetResultWhereEmpty()
        {
            var defaultResult = new[] { Selector(Form.BaseSet), Selector(Form.More) };
            var expected = new[] { defaultResult[1] };
            var eventBuffer = new EventBuffer();
            var formNodeCollections =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
                {
                    {
                        defaultResult[0],
                        Mock.Of<IBufferingEventViewProvider<INodeCollection<Modifier>>>(p =>
                            p.DefaultView == new NodeCollection<Modifier>(eventBuffer) { { null, null } })
                    },
                    {
                        defaultResult[1],
                        Mock.Of<IBufferingEventViewProvider<INodeCollection<Modifier>>>(p =>
                            p.DefaultView == new NodeCollection<Modifier>(eventBuffer))
                    }
                };
            var statGraph = Mock.Of<IReadOnlyStatGraph>(g => g.FormNodeCollections == formNodeCollections);
            var defaultRuleSet = Mock.Of<IGraphPruningRuleSet>(r =>
                r.SelectRemovableNodesByForm(statGraph) == defaultResult);
            var sut = CreateSut(defaultRuleSet);

            var actual = sut.SelectRemovableNodesByForm(statGraph);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(true, true, true, ExpectedResult = true)]
        [TestCase(false, true, true, ExpectedResult = false)]
        [TestCase(true, false, true, ExpectedResult = false)]
        [TestCase(true, true, false, ExpectedResult = false)]
        public bool SelectRemovableStatsReturnsCorrectResult(
            bool nodesCanBeRemoved, bool formNodesCanBeRemoved, bool pathsCanBeRemoved)
        {
            var nodeCollection = new Dictionary<NodeSelector, IBufferingEventViewProvider<ICalculationNode>>();
            if (!nodesCanBeRemoved)
            {
                nodeCollection.Add(Selector(NodeType.Total), MockProvider<ICalculationNode>());
            }
            var formNodeCollections =
                new Dictionary<FormNodeSelector, IBufferingEventViewProvider<INodeCollection<Modifier>>>
                {
                    { Selector(Form.More), MockProvider<INodeCollection<Modifier>>() }
                };
            var removableFormNodes = formNodesCanBeRemoved
                ? new[] { Selector(Form.More) }
                : new FormNodeSelector[0];
            var paths = MockProvider<IObservableCollection<PathDefinition>>();
            var nodeRemovalDeterminer = Mock.Of<IDeterminesNodeRemoval>(d =>
                d.CanBeRemoved(paths) == pathsCanBeRemoved);
            var statGraph = Mock.Of<IReadOnlyStatGraph>(g =>
                g.Nodes == nodeCollection &&
                g.FormNodeCollections == formNodeCollections &&
                g.Paths == paths);
            var defaultRuleSet = Mock.Of<IGraphPruningRuleSet>(r =>
                r.SelectRemovableNodesByForm(statGraph) == removableFormNodes);
            var sut = CreateSut(defaultRuleSet, nodeRemovalDeterminer);

            return sut.CanStatGraphBeRemoved(statGraph);
        }

        private static UserSpecifiedValuePruningRuleSet CreateSut(
            IGraphPruningRuleSet defaultRuleSet = null, IDeterminesNodeRemoval nodeRemovalDeterminer = null)
            => new UserSpecifiedValuePruningRuleSet(defaultRuleSet ?? Mock.Of<IGraphPruningRuleSet>(),
                nodeRemovalDeterminer ?? Mock.Of<IDeterminesNodeRemoval>());

        private static IBufferingEventViewProvider<T> MockProvider<T>()
            => Mock.Of<IBufferingEventViewProvider<T>>();
    }
}