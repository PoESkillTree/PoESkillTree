using System;
using System.ComponentModel;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Behaviors;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [TestFixture]
    public class BasicCalculatorTest
    {
        [Test]
        public void SimpleCalculation()
        {
            var sut = Calculator.Create();

            var expected = new NodeValue(5);

            sut.NewBatchUpdate()
                .AddModifier(Stat, Form.BaseAdd, new Constant(expected))
                .DoUpdate();

            var actual = sut.NodeRepository.GetNode(Stat).Value;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void MultipleUpdates()
        {
            var sut = Calculator.Create();
            var source = new ModifierSource.Global();
            var removedModifier = new Modifier(new[] { Stat }, Form.BaseAdd, new Constant(100), source);

            sut.NewBatchUpdate()
                .AddModifier(Stat, Form.BaseAdd, new Constant(10), source)
                .DoUpdate();
            sut.NewBatchUpdate()
                .AddModifier(Stat, Form.BaseAdd, new Constant(1), source)
                .AddModifier(removedModifier)
                .DoUpdate();
            sut.NewBatchUpdate()
                .RemoveModifier(removedModifier)
                .AddModifier(Stat, Form.BaseAdd, new Constant(1000), source)
                .DoUpdate();

            var actual = sut.NodeRepository.GetNode(Stat).Value;

            Assert.AreEqual(new NodeValue(1011), actual);
        }

        [Test]
        public void EvasionCalculation()
        {
            var sut = Calculator.Create();

            var evasionStat = new Stat("evasion");
            var lvlStat = new Stat("lvl");
            var dexterityStat = new Stat("dexterity");
            var globalSource = new ModifierSource.Global();
            var bodySource = new ModifierSource.Local.Item(ItemSlot.BodyArmour);
            var shieldSource = new ModifierSource.Local.Item(ItemSlot.OffHand);

            sut.NewBatchUpdate()
                .AddModifier(evasionStat, Form.BaseSet, new Constant(53), globalSource)
                .AddModifier(evasionStat, Form.BaseAdd, new PerStatValue(lvlStat, 3), globalSource)
                .AddModifier(evasionStat, Form.BaseSet, new Constant(1000), bodySource)
                .AddModifier(evasionStat, Form.BaseSet, new Constant(500), shieldSource)
                .AddModifier(evasionStat, Form.Increase, new Constant(100), globalSource)
                .AddModifier(evasionStat, Form.Increase, new PerStatValue(dexterityStat, 1, 5), globalSource)
                .AddModifier(evasionStat, Form.Increase, new Constant(20), shieldSource)
                .AddModifier(evasionStat, Form.More, new Constant(100), bodySource)
                .AddModifier(lvlStat, Form.BaseSet, new Constant(90), globalSource)
                .AddModifier(dexterityStat, Form.BaseSet, new Constant(32), globalSource)
                .AddModifier(dexterityStat, Form.BaseAdd, new Constant(50), globalSource)
                .DoUpdate();
            var lvlTotal = 90 * 3;
            var dexterityTotal = 50 + 32;
            var globalBase = 53 + lvlTotal;
            var globalIncrease = 1 + Math.Ceiling(dexterityTotal / 5.0) / 100;
            var globalTotal = globalBase * (1 + globalIncrease);
            var bodyBase = 1000;
            var bodyTotal = bodyBase * (1 + globalIncrease) * 2;
            var shieldBase = 500;
            var shieldTotal = shieldBase * (1 + globalIncrease + 0.2);
            var evasionTotal = globalTotal + bodyTotal + shieldTotal;

            var actual = sut.NodeRepository.GetNode(evasionStat).Value;

            Assert.AreEqual(new NodeValue(evasionTotal), actual);
        }

        [TestCase(5)]
        [TestCase(15)]
        [TestCase(25)]
        public void Clip(double value)
        {
            var sut = Calculator.Create();

            sut.NewBatchUpdate()
                .AddModifier(Stat, Form.BaseAdd, new Constant(value))
                .AddModifier(Stat.Minimum, Form.BaseAdd, new Constant(10))
                .AddModifier(Stat.Maximum, Form.BaseAdd, new Constant(20))
                .DoUpdate();
            var expected = new NodeValue(Math.Max(Math.Min(value, 20), 10));

            var actual = sut.NodeRepository.GetNode(Stat).Value;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Pruning()
        {
            var sut = Calculator.Create();
            var mod = new Modifier(new[] { Stat }, Form.BaseAdd, new Constant(1), new ModifierSource.Global());

            sut.NewBatchUpdate().AddModifier(mod).DoUpdate();

            var removedNode = sut.NodeRepository.GetNode(Stat);
            var keptNode = sut.NodeRepository.GetNode(Stat, NodeType.Subtotal);
            keptNode.ValueChanged += (sender, args) => { };

            sut.NewBatchUpdate().RemoveModifier(mod).DoUpdate();

            Assert.AreEqual(keptNode, sut.NodeRepository.GetNode(Stat, NodeType.Subtotal));
            Assert.AreNotEqual(removedNode, sut.NodeRepository.GetNode(Stat));
        }

        [Test]
        public void Events()
        {
            var sut = Calculator.Create();
            var mod = new Modifier(new[] { Stat }, Form.BaseAdd, new Constant(1), new ModifierSource.Global());
            var node = sut.NodeRepository.GetNode(Stat);
            var invocations = 0;
            node.ValueChanged += (sender, args) => invocations++;

            Assert.IsNull(node.Value);
            sut.NewBatchUpdate().AddModifier(mod).DoUpdate();
            Assert.AreEqual(new NodeValue(1), node.Value);
            sut.NewBatchUpdate().AddModifier(mod).DoUpdate();
            Assert.AreEqual(new NodeValue(2), node.Value);
            sut.NewBatchUpdate().AddModifier(mod).DoUpdate();
            Assert.AreEqual(new NodeValue(3), node.Value);

            Assert.AreEqual(3, invocations);
        }

        [Test]
        public void ExplicitlyRegistered()
        {
            var sut = Calculator.Create();
            var modifierStat = new Stat("m");
            var registeredStat =
                new Stat("r", explicitRegistrationType: ExplicitRegistrationTypes.UserSpecifiedValue(0));
            var value = new StatValue(registeredStat);
            IStat actual = null;
            sut.ExplicitlyRegisteredStats.CollectionChanged += (sender, args) =>
            {
                Assert.IsNull(actual);
                Assert.AreEqual(CollectionChangeAction.Add, args.Action);
                actual = (((ICalculationNode, IStat)) args.Element).Item2;
            };

            sut.NewBatchUpdate().AddModifier(modifierStat, Form.BaseAdd, value).DoUpdate();
            Assert.IsNull(actual);
            var _ = sut.NodeRepository.GetNode(modifierStat).Value;

            Assert.AreEqual(registeredStat, actual);
        }

        [Test]
        public void Behavior()
        {
            var sut = Calculator.Create();
            var transformedStat = new Stat("transformed");
            var behavior = new Behavior(new[] { transformedStat }, new[] { NodeType.Subtotal },
                BehaviorPathInteraction.All, new ValueTransformation(_ => new Constant(5)));
            var stat = new Stat("stat", behaviors: new[] { behavior });

            sut.NewBatchUpdate().AddModifier(stat, Form.BaseAdd, new Constant(1)).DoUpdate();

            var actual = sut.NodeRepository.GetNode(transformedStat).Value;

            Assert.AreEqual(new NodeValue(5), actual);
        }

        private static readonly IStat Stat = new Stat("stat");
    }
}