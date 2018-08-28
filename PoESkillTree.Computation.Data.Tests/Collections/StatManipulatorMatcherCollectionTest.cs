using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Data.Collections;

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
            _sut = new StatManipulatorMatcherCollection(new ModifierBuilderStub());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void AddWithoutSubstitution()
        {
            StatConverter manipulator = s => null;

            _sut.Add(Regex, manipulator);

            var builder = _sut.AssertSingle(Regex);
            Assert.AreSame(manipulator, builder.StatConverter);
        }

        [Test]
        public void AddWithSubstitution()
        {
            StatConverter manipulator = s => null;

            _sut.Add(Regex, manipulator, "substitution");

            var builder = _sut.AssertSingle(Regex, "substitution");
            Assert.AreSame(manipulator, builder.StatConverter);
        }
    }
}