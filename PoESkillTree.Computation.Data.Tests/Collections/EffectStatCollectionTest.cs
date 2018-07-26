using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class EffectStatCollectionTest
    {
        private EffectStatCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new EffectStatCollection();
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
            var value = 5;

            _sut.Add(effect, form, stat, value);

            var data = _sut.Single();
            Assert.AreSame(expectedStat, data.Stat);
            Assert.AreSame(form, data.Form);
            Assert.AreEqual(value, data.Value);
        }
    }
}