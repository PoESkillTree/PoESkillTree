using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel.PassiveTree;
using POESKillTree.Computation.Model;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace PoESkillTree.Tests.Computation.Model
{
    [TestFixture]
    public class ComputationObservablesTest
    {
        [Test]
        public async Task InitialParseReturnsCorrectResult()
        {
            var expected = CreateModifiers(10);
            var passiveTree = CreatePassiveTree(3);
            var givenModifierParseDelegates = new Func<IReadOnlyList<Modifier>>[]
            {
                () => expected.GetRange(0, 1),
                () => expected.GetRange(1, 3),
            };
            var parseResults = new[]
            {
                ParseResult.Success(expected.GetRange(4, 2)),
                ParseResult.Success(expected.GetRange(6, 1)),
                ParseResult.Success(expected.GetRange(7, 3)),
            };
            var parser = Mock.Of<IParser>(p =>
                p.ParsePassiveNode(0) == parseResults[0] &&
                p.ParsePassiveNode(1) == parseResults[1] &&
                p.ParsePassiveNode(2) == parseResults[2] &&
                p.CreateGivenModifierParseDelegates() == givenModifierParseDelegates);
            var sut = CreateSut(parser);

            var actual = await AggregateAsync(sut.InitialParse(passiveTree, TimeSpan.Zero));

            Assert.That(actual.AddedModifiers, Is.EquivalentTo(expected));
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public async Task ParseSkilledPassiveNodesReturnsCorrectResult()
        {
            var expected = CreateModifiers(6);
            var skilledNodes = CreateSkillNodes(3);
            var parseResults = new[]
            {
                ParseResult.Success(expected.GetRange(0, 2)),
                ParseResult.Success(expected.GetRange(2, 1)),
                ParseResult.Success(expected.GetRange(3, 3)),
            };
            var parser = Mock.Of<IParser>(p =>
                p.ParseSkilledPassiveNode(0) == parseResults[0] &&
                p.ParseSkilledPassiveNode(1) == parseResults[1] &&
                p.ParseSkilledPassiveNode(2) == parseResults[2]);
            var sut = CreateSut(parser);

            var actual = await AggregateAsync(sut.ParseSkilledPassiveNodes(skilledNodes));

            Assert.AreEqual(expected, actual.AddedModifiers);
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public void ObserveSkilledPassiveNodesReturnsCorrectResult()
        {
            var modifiers = CreateModifiers(6);
            var skilledNodes = CreateSkillNodes(3);
            var parseResults = new[]
            {
                ParseResult.Success(modifiers.GetRange(0, 2)),
                ParseResult.Success(modifiers.GetRange(2, 1)),
                ParseResult.Success(modifiers.GetRange(3, 3)),
            };
            var expected = new[]
            {
                new CalculatorUpdate(modifiers, new Modifier[0]),
                new CalculatorUpdate(new Modifier[0], parseResults[0].Modifiers),
                new CalculatorUpdate(new Modifier[0], parseResults[1].Modifiers),
            };
            var parser = Mock.Of<IParser>(p =>
                p.ParseSkilledPassiveNode(0) == parseResults[0] &&
                p.ParseSkilledPassiveNode(1) == parseResults[1] &&
                p.ParseSkilledPassiveNode(2) == parseResults[2]);
            var observableSet = new ObservableSet<SkillNode>();
            var sut = CreateSut(parser);

            var actual = new List<CalculatorUpdate>();
            sut.ObserveSkilledPassiveNodes(observableSet).Subscribe(actual.Add);
            observableSet.UnionWith(skilledNodes);
            observableSet.Remove(skilledNodes[0]);
            observableSet.Remove(skilledNodes[1]);

            Assert.AreEqual(expected, actual);
        }

        private static PassiveTreeDefinition CreatePassiveTree(int nodeCount)
        {
            var nodes = Enumerable.Range(0, nodeCount).Select(id => CreatePassiveNode((ushort) id));
            return new PassiveTreeDefinition(nodes.ToList());
        }

        private static PassiveNodeDefinition CreatePassiveNode(ushort id)
            => new PassiveNodeDefinition(id, default, "", false, false,
                0, new string[0]);

        private static IReadOnlyList<SkillNode> CreateSkillNodes(int nodeCount)
            => Enumerable.Range(0, nodeCount).Select(id => new SkillNode { Id = (ushort) id }).ToList();

        private static List<Modifier> CreateModifiers(int count)
            => Enumerable.Range(0, count).Select(i => CreateModifier(i.ToString())).ToList();

        private static Modifier CreateModifier(string statIdentity)
            => new Modifier(new[] { new Stat(statIdentity) }, default, null, null);

        private static ComputationObservables CreateSut(IParser parser)
            => new ComputationObservables(parser);

        private static async Task<CalculatorUpdate> AggregateAsync(IObservable<CalculatorUpdate> observable)
            => await observable
                .Aggregate(Accumulate)
                .SingleAsync();

        private static CalculatorUpdate Accumulate(CalculatorUpdate l, CalculatorUpdate r)
            => new CalculatorUpdate(
                l.AddedModifiers.Concat(r.AddedModifiers).ToList(),
                l.RemovedModifiers.Concat(r.RemovedModifiers).ToList());
    }
}