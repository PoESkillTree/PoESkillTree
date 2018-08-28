using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    [TestFixture]
    public class ConversionTest
    {
        // This test tests the behaviors necessary for stat conversions
        // (located in PoESkillTree.Computation.Builders.Behaviors).

        private ICalculator _sut;
        private IStat _bar;
        private IStat _foo;
        private IStat _barFooConversion;
        private IStat _barFooGain;
        private IStat _barConversion;
        private IStat _barSkillConversion;

        [SetUp]
        public void SetUp()
        {
            _sut = Calculator.CreateCalculator();
            _bar = new Stat("Bar");
            _foo = new Stat("Foo");
            var statFactory = new StatFactory();
            _barFooConversion = statFactory.ConvertTo(_bar, _foo);
            _barFooGain = statFactory.GainAs(_bar, _foo);
            _barConversion = statFactory.Conversion(_bar);
            _barSkillConversion = statFactory.SkillConversion(_bar);
        }

        [Test]
        public void SimpleGain()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(_barFooGain, Form.BaseAdd, 50)
                .DoUpdate();

            Assert.AreEqual(new NodeValue(1.5), GetValue(_foo));
        }

        [Test]
        public void SimpleConversion()
        {
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(new[] { _barFooConversion, _barConversion, _barSkillConversion }, Form.BaseAdd, 100.0 / 3)
                .DoUpdate();

            Assert.AreEqual(new NodeValue(1), GetValue(_foo));
            Assert.AreEqual(new NodeValue(2), GetValue(_bar));
        }

        [Test]
        public void Complex()
        {
            var barFooConversion = new[] { _barFooConversion, _barConversion, _barSkillConversion };
            var localSource = new ModifierSource.Local.Given();
            var skillSource = new ModifierSource.Local.Skill();
            _sut.NewBatchUpdate()
                .AddModifier(_bar, Form.BaseAdd, 3)
                .AddModifier(_bar, Form.BaseAdd, 2, localSource)
                .AddModifier(barFooConversion, Form.BaseAdd, 50, skillSource)
                .AddModifier(barFooConversion, Form.BaseAdd, 30)
                .AddModifier(barFooConversion, Form.BaseAdd, 30)
                .AddModifier(_barFooGain, Form.BaseAdd, 20)
                .AddModifier(_foo, Form.Increase, 50)
                .AddModifier(_foo, Form.Increase, 50, localSource)
                .AddModifier(_foo, Form.BaseAdd, 1)
                .AddModifier(_foo, Form.BaseAdd, 2, localSource)
                .DoUpdate();

            var globalPath = (3 * (1 + 0.2) + 1) * 1.5;
            var localPath = (2 * (1 + 0.2) + 2) * 2;
            Assert.AreEqual(new NodeValue(110), GetValue(_barConversion));
            Assert.AreEqual(new NodeValue(50), GetValue(_barSkillConversion));
            Assert.AreEqual(new NodeValue(0), GetValue(_bar));
            Assert.AreEqual(new NodeValue(20), GetValue(_barFooGain));
            Assert.AreEqual(new NodeValue(100), GetValue(_barFooConversion));
            Assert.AreEqual(new NodeValue(globalPath + localPath), GetValue(_foo));
        }

        private NodeValue? GetValue(IStat stat) => _sut.NodeRepository.GetNode(stat).Value;
    }
}