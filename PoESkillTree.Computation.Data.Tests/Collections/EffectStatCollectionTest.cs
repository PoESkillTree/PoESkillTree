using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;

namespace PoESkillTree.Computation.Data.Collections
{
    [TestFixture]
    public class EffectStatCollectionTest
    {
        private Mock<IValueBuilders> _valueFactory;
        private EffectStatCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _valueFactory = new Mock<IValueBuilders>();
            _sut = new EffectStatCollection(new ModifierBuilderStub(), _valueFactory.Object);
        }

        [Test]
        public void IsEmpty()
        {
            CollectionAssert.IsEmpty(_sut);
        }

        [Test]
        public void AddAddsCorrectData()
        {
            var expectedStat = Mock.Of<IStatBuilder>();
            var stat = Mock.Of<IStatBuilder>();
            var effect = Mock.Of<IEffectBuilder>(b => b.AddStat(stat) == expectedStat);
            var form = Mock.Of<IFormBuilder>();
            var value = Mock.Of<IValueBuilder>();
            _valueFactory.Setup(v => v.Create(3)).Returns(value);

            _sut.Add(effect, form, stat, 3);

            var builder = _sut.AssertSingle();
            Assert.That(builder.Forms, Has.Exactly(1).SameAs(form));
            Assert.That(builder.Stats, Has.Exactly(1).SameAs(expectedStat));
            Assert.That(builder.Values, Has.Exactly(1).SameAs(value));
        }
    }
}