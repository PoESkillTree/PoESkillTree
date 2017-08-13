using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Effects;
using PoESkillTree.Computation.Providers.Stats;

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
        public void AddStringsAddsCorrectData()
        {
            var effect = Mock.Of<IEffectProvider>();

            _sut.Add(effect, "s1", "s2", "s3");

            var data = _sut.Single();
            Assert.AreSame(effect, data.Effect);
            CollectionAssert.AreEqual(new[] { "s1", "s2", "s3" }, data.StatLines);
            CollectionAssert.IsEmpty(data.FlagStats);
        }

        [Test]
        public void AddFlagsAddsCorrectData()
        {
            var effect = Mock.Of<IEffectProvider>();
            var flags = new[]
            {
                Mock.Of<IFlagStatProvider>(), Mock.Of<IFlagStatProvider>(),
                Mock.Of<IFlagStatProvider>()
            };

            _sut.Add(effect, flags);

            var data = _sut.Single();
            Assert.AreSame(effect, data.Effect);
            CollectionAssert.IsEmpty(data.StatLines);
            CollectionAssert.AreEqual(flags, data.FlagStats);
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var effect = Mock.Of<IEffectProvider>();
            var flags = new[]
            {
                Mock.Of<IFlagStatProvider>(), Mock.Of<IFlagStatProvider>(),
                Mock.Of<IFlagStatProvider>()
            };

            _sut.Add(effect, "s1");
            _sut.Add(effect, "s1", "s2", "s3");
            _sut.Add(effect, flags);

            Assert.AreEqual(3, _sut.Count());
        }
    }
}