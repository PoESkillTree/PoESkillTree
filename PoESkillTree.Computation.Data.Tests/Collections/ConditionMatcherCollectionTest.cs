using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Data.Collections;

namespace PoESkillTree.Computation.Data.Tests.Collections
{
    [TestFixture]
    public class ConditionMatcherCollectionTest
    {
        private const string Regex = "regex";

        private ConditionMatcherCollection _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ConditionMatcherCollection(new ModifierBuilderStub());
        }

        [Test]
        public void IsEmpty()
        {
            Assert.IsEmpty(_sut);
        }

        [Test]
        public void Add()
        {
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, condition);

            var builder = _sut.AssertSingle(Regex);
            Assert.That(builder.Conditions, Has.Exactly(1).SameAs(condition));
        }

        [Test]
        public void AddManyAddsToCount()
        {
            var condition = Mock.Of<IConditionBuilder>();

            _sut.Add(Regex, condition);
            _sut.Add(Regex, condition);
            _sut.Add(Regex, (condition, condition));

            Assert.AreEqual(3, _sut.Count());
        }
    }
}