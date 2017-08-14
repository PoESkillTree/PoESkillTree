using Moq;
using NUnit.Framework;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ToStringParserDecoratorTest
    {
        [Test]
        public void IsIParserString()
        {
            var sut = new ToStringParserDecorator<int>(null);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [Test]
        public void TryParseReturnsTrueIfInjectionReturnsTrue()
        {
            const string stat = "known";
            var injection = new Mock<IParser<int>>();
            int result;
            injection.Setup(p => p.TryParse(stat, out result)).Returns(true);
            var sut = new ToStringParserDecorator<int>(injection.Object);

            var actual = sut.TryParse(stat, out var _);

            Assert.True(actual);
        }

        [Test]
        public void TryParseReturnsFalseIfInjectionReturnsFalse()
        {
            const string stat = "unknown";
            var injection = new Mock<IParser<int>>();
            int result;
            injection.Setup(p => p.TryParse(stat, out result)).Returns(false);
            var sut = new ToStringParserDecorator<int>(injection.Object);

            var actual = sut.TryParse(stat, out var _);

            Assert.False(actual);
        }

        [Test]
        public void TryParseCallsToStringIfInjectionReturnsTrue()
        {
            const string stat = "known";
            var injection = new Mock<IParser<int>>();
            int expected = 3;
            injection.Setup(p => p.TryParse(stat, out expected)).Returns(true);
            var sut = new ToStringParserDecorator<int>(injection.Object);

            sut.TryParse(stat, out var actual);

            Assert.AreEqual(expected.ToString(), actual);
        }

        [Test]
        public void TryParseCallsToStringIfInjectionReturnsFalse()
        {
            const string stat = "unknown";
            var injection = new Mock<IParser<int>>();
            int expected = 3;
            injection.Setup(p => p.TryParse(stat, out expected)).Returns(false);
            var sut = new ToStringParserDecorator<int>(injection.Object);

            sut.TryParse(stat, out var actual);

            Assert.AreEqual(expected.ToString(), actual);
        }

        [Test]
        public void TryParseOutputsNullOfInjectionReturnsFalseAndOutputsNull()
        {
            const string stat = "unknown";
            var injection = new Mock<IParser<object>>();
            object expected;
            injection.Setup(p => p.TryParse(stat, out expected)).Returns(false);
            var sut = new ToStringParserDecorator<object>(injection.Object);

            sut.TryParse(stat, out var actual);

            Assert.Null(actual);
        }
    }
}