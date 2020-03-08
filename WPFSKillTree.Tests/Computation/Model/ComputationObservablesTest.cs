using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EnumsNET;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Model
{
    [TestFixture]
    public class ComputationObservablesTest
    {
        [Test]
        public async Task InitialParseReturnsCorrectResult()
        {
            var expected = CreateModifiers(10);
            var passiveTree = CreatePassiveTree(3);
            var parseResults = CreateParseResults(expected);
            var givenModifierParseDelegates = new Func<IReadOnlyList<Modifier>>[]
            {
                () => expected.GetRange(6, 1),
                () => expected.GetRange(7, 3),
            };
            var parser = Mock.Of<IParser>(p =>
                p.ParsePassiveNode(0) == parseResults[0] &&
                p.ParsePassiveNode(1) == parseResults[1] &&
                p.ParsePassiveNode(2) == parseResults[2] &&
                p.CreateGivenModifierParseDelegates() == givenModifierParseDelegates);
            var sut = CreateSut(parser);

            var actual =
                await AggregateAsync(sut.InitialParse(passiveTree, TimeSpan.Zero, ImmediateScheduler.Instance));

            Assert.That(actual.AddedModifiers, Is.EquivalentTo(expected));
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public async Task ParseSkilledPassiveNodesReturnsCorrectResult()
        {
            var expected = CreateModifiers(6);
            var skilledNodes = CreateSkillNodes(3);
            var parseResults = CreateParseResults(expected);
            var sut = CreateSut(MockSkilledPassiveNodeParser(parseResults));

            var actual = await AggregateAsync(sut.ParseSkilledPassiveNodes(skilledNodes));

            Assert.AreEqual(expected, actual.AddedModifiers);
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public void ObserveSkilledPassiveNodesGeneratesCorrectValues()
        {
            var modifiers = CreateModifiers(6);
            var skilledNodes = CreateSkillNodes(3);
            var parseResults = CreateParseResults(modifiers);
            var expected = new[]
            {
                new CalculatorUpdate(modifiers, new Modifier[0]),
                new CalculatorUpdate(new Modifier[0], parseResults[0].Modifiers),
                new CalculatorUpdate(new Modifier[0], parseResults[1].Modifiers),
            };
            var observableSet = new ObservableSet<SkillNode>();
            var sut = CreateSut(MockSkilledPassiveNodeParser(parseResults));

            var actual = new List<CalculatorUpdate>();
            using (sut.ObserveSkilledPassiveNodes(observableSet).Subscribe(actual.Add))
            {
                observableSet.UnionWith(skilledNodes);
                observableSet.Remove(skilledNodes[0]);
                observableSet.Remove(skilledNodes[1]);
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ParseItemsReturnsCorrectResult()
        {
            var modifiers = CreateModifiers(6);
            var items = CreateItems(2);
            var parseResults = CreateParseResults(modifiers);
            var expected = parseResults.Take(2).SelectMany(r => r.Modifiers).ToList();
            var parser = MockItemParser(items, parseResults);
            var sut = CreateSut(parser);

            var actual = await AggregateAsync(sut.ParseItems(items));

            Assert.AreEqual(expected, actual.AddedModifiers);
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public void ObserveItemsGeneratesCorrectValues()
        {
            var modifiers = CreateModifiers(6);
            var items = CreateItems(2);
            var parseResults = CreateParseResults(modifiers);
            var expected = new[]
            {
                new CalculatorUpdate(parseResults[0].Modifiers, new Modifier[0]),
                new CalculatorUpdate(parseResults[1].Modifiers, new Modifier[0]),
                new CalculatorUpdate(new Modifier[0], parseResults[0].Modifiers),
            };
            var parser = MockItemParser(items, parseResults);
            var sut = CreateSut(parser);
            var observableCollection = new ObservableSet<(Item, ItemSlot)>();

            var actual = new List<CalculatorUpdate>();
            using (sut.ObserveItems(observableCollection).Subscribe(actual.Add))
            {
                observableCollection.Add(items[0]);
                observableCollection.Add(items[1]);
                observableCollection.Remove(items[0]);
            }

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ParseSkillsReturnsCorrectResult()
        {
            var expected = CreateModifiers(6);
            var skills = CreateSkills();
            var parseResults = CreateParseResults(expected);
            var parser = MockSkillParser(skills, parseResults);
            var sut = CreateSut(parser);

            var actual = await AggregateAsync(sut.ParseSkills(skills));

            Assert.AreEqual(expected, actual.AddedModifiers);
            Assert.IsEmpty(actual.RemovedModifiers);
        }

        [Test]
        public void ObserveSkillsGeneratesCorrectValues()
        {
            var modifiers = CreateModifiers(6);
            var skills = CreateSkills();
            var parseResults = CreateParseResults(modifiers);
            var expected = new[]
            {
                new CalculatorUpdate(parseResults[0].Modifiers, new Modifier[0]),
                new CalculatorUpdate(new Modifier[0], parseResults[0].Modifiers),
            };
            var parser = MockSkillParser(skills, parseResults);
            var sut = CreateSut(parser);
            var observableCollection = new ObservableSet<IReadOnlyList<Skill>>();

            var actual = new List<CalculatorUpdate>();
            using (sut.ObserveSkills(observableCollection).Subscribe(actual.Add))
            {
                observableCollection.Add(skills[0]);
                observableCollection.Remove(skills[0]);
            }

            Assert.AreEqual(expected, actual);
        }

        private static IParser MockSkilledPassiveNodeParser(IReadOnlyList<ParseResult> parseResults)
        {
            var parser = new Mock<IParser>();
            for (ushort i = 0; i < parseResults.Count; i++)
            {
                var id = i;
                parser.Setup(p => p.ParseSkilledPassiveNode(id)).Returns(parseResults[i]);
            }
            return parser.Object;
        }

        private static IParser MockItemParser(
            IReadOnlyList<(Item, ItemSlot)> items, IReadOnlyList<ParseResult> parseResults)
        {
            var parser = new Mock<IParser>();
            var itemSlots = Enums.GetValues<ItemSlot>().ToList();
            for (var i = 0; i < itemSlots.Count; i++)
            {
                var slot = itemSlots[i];
                if (i < items.Count)
                {
                    var item = items[i].Item1;
                    parser.Setup(p => p.ParseItem(item, slot, Entity.Character)).Returns(parseResults[i]);
                }
            }
            return parser.Object;
        }

        private static IParser MockSkillParser(
            IReadOnlyList<IReadOnlyList<Skill>> skills, IReadOnlyList<ParseResult> parseResults)
        {
            var parser = new Mock<IParser>();
            for (var i = 0; i < skills.Count; i++)
            {
                var id = i;
                parser.Setup(p => p.ParseSkills(skills[id], Entity.Character)).Returns(parseResults[i]);
            }
            return parser.Object;
        }

        private static PassiveTreeDefinition CreatePassiveTree(int nodeCount)
        {
            var nodes = Enumerable.Range(0, nodeCount).Select(id => CreatePassiveNode((ushort) id));
            return new PassiveTreeDefinition(nodes.ToList());
        }

        private static PassiveNodeDefinition CreatePassiveNode(ushort id)
            => new PassiveNodeDefinition(id, default, "", false, false,
                0, default, new string[0]);

        private static IReadOnlyList<SkillNode> CreateSkillNodes(int nodeCount)
            => Enumerable.Range(0, nodeCount).Select(id => new SkillNode { Id = (ushort) id }).ToList();

        private static IReadOnlyList<(Item, ItemSlot)> CreateItems(int count)
            => Enumerable.Range(0, count).Zip(Enums.GetValues<ItemSlot>(), (i, s) => (CreateItem(i), s)).ToList();

        private static Item CreateItem(int id)
            => new Item(id.ToString(), "", 0, 0, default, false, new string[0], true);

        private static IReadOnlyList<IReadOnlyList<Skill>> CreateSkills()
            => Enumerable.Range(0, 3).Select(i => Enumerable.Range(i, 2).Select(CreateSkill).ToList()).ToList();

        private static Skill CreateSkill(int id) =>
            Skill.FromGem(new Gem(id.ToString(), 1, 0, default, 0, 0, true), true);

        private static List<Modifier> CreateModifiers(int count)
            => Enumerable.Range(0, count).Select(i => CreateModifier(i.ToString())).ToList();

        private static Modifier CreateModifier(string statIdentity)
            => new Modifier(new[] {new Stat(statIdentity)}, default, new Constant(true), new ModifierSource.Global());

        private static IReadOnlyList<ParseResult> CreateParseResults(List<Modifier> modifiers)
            => new[]
            {
                ParseResult.Success(modifiers.GetRange(0, 2)),
                ParseResult.Success(modifiers.GetRange(2, 1)),
                ParseResult.Success(modifiers.GetRange(3, 3)),
            };

        private static ComputationObservables CreateSut(IParser parser)
            => new ComputationObservables(parser);

        private static async Task<CalculatorUpdate> AggregateAsync(IObservable<CalculatorUpdate> observable)
            => await observable
                .Aggregate(CalculatorUpdate.Accumulate)
                .SingleAsync();
    }
}