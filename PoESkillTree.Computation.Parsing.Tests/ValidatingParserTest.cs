using Moq;
using NUnit.Framework;
using PoESkillTree.Common.Model.Items;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ValidatingParserTest
    {
        [Test]
        public void IsIParser()
        {
            var sut = new ValidatingParser<string>(null);

            Assert.IsInstanceOf<IParser<string>>(sut);
        }

        [Test]
        public void TryParsePassesOutputsOnUnchanged()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = "remaining";
            var result = "result";
            innerMock.Setup(p => p.TryParse("stat", out remaining, out result))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var actualRemaining, out var actualResult);

            Assert.AreEqual(remaining, actualRemaining);
            Assert.AreEqual(result, actualResult);
        }

        [Test]
        public void TryParseReturnsFalseIfInnerReturnsFalse()
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out _, out _))
                .Returns(false);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsFalseIfRemainingContainsNonWhiteSpace()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = " r ";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsFalseIfRemainingStartsWithHidden()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = ItemConstants.HiddenStatSuffix + " ";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.False(actual);
        }

        [Test]
        public void TryParseReturnsTrueIfRemainingIsOnlyWhiteSpace()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = " \n\t\r";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.True(actual);
        }

        [Test]
        public void TryParseReturnsTrueIfRemainingIsOnlyWhiteSpaceAndEndsWithHidden()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = " \n\t\r " + ItemConstants.HiddenStatSuffix;
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.True(actual);
        }

        [Test]
        public void TryParseHiddenTestIsCaseInsensitive()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = ItemConstants.HiddenStatSuffix.ToLowerInvariant();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            var actual = sut.TryParse("stat", out var _, out var _);

            Assert.True(actual);
        }
    }
}