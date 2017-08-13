using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.Computation.Providers.Stats;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class StatManipulatorMatcherCollectionTest
    {
        private const string Regex = "regex";

        private StatManipulatorMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new StatManipulatorMatcherCollection(new MatchBuilderStub());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddWithoutSubstitution()
        {
            Func<IStatProvider, IStatProvider> manipulator = s => null;

            _sut.Add(Regex, manipulator);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(manipulator, builder.StatConverter);
        }

        [Test]
        public void AddWithSubstitution()
        {
            Func<IStatProvider, IStatProvider> manipulator = s => null;

            _sut.Add(Regex, manipulator, "substitution");

            var builder = _sut.AssertSingle(Regex, "substitution");
            Assert.AreSame(manipulator, builder.StatConverter);
        }

        [Test]
        public void AddGeneric()
        {
            var inputStat = Mock.Of<IPoolStatProvider>();
            var resultStat = Mock.Of<IPoolStatProvider>();
            var converterMock = new Mock<Func<IPoolStatProvider, IStatProvider>>();
            converterMock.Setup(c => c(inputStat)).Returns(() => resultStat);

            _sut.Add(Regex, converterMock.Object, "substitution");

            var builder = _sut.AssertSingle(Regex, "substitution");
            var actualConverter = builder.StatConverter;
            Assert.AreSame(resultStat, actualConverter(inputStat));
            Assert.Throws<NotSupportedException>(() => actualConverter(Mock.Of<IStatProvider>()));
        }
    }
}