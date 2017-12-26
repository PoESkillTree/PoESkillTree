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
        public void TryParsePassesResultOnUnchanged()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = "remaining";
            var result = "result";
            innerMock.Setup(p => p.TryParse("stat", out remaining, out result))
                .Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var _, out var actualResult);

            Assert.AreEqual(result, actualResult);
        }

        [Test]
        public void TryParseReturnsFalseIfInnerReturnsFalse()
        {
            var innerMock = new Mock<IParser<string>>();
            var remaining = "remaining";
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _))
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

        [TestCase("a b c")]
        [TestCase(ItemConstants.HiddenStatSuffix + " a")]
        public void TryParseOutputsCorrectUnchangedRemaining(string remaining)
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out remaining, out _)).Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var actualRemaining, out var _);

            Assert.AreEqual(remaining, actualRemaining);
        }

        [TestCase(" r ", ExpectedResult = "r")]
        [TestCase(ItemConstants.HiddenStatSuffix + " ", ExpectedResult = ItemConstants.HiddenStatSuffix)]
        [TestCase(" \n\t\r", ExpectedResult = "")]
        [TestCase(" \n\t\r " + ItemConstants.HiddenStatSuffix, ExpectedResult = "")]
        [TestCase(" \ntest\t\r " + ItemConstants.HiddenStatSuffix, ExpectedResult = "test")]
        public string TryParseOutputsCorrectRemaining(string innerRemaining)
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            innerMock.Setup(p => p.TryParse("stat", out innerRemaining, out _)).Returns(true);
            var sut = new ValidatingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var remaining, out var _);

            return remaining;
        }

        [Test]
        public void TryParseCleansIfInnerReturnsFalse()
        {
            var innerMock = new Mock<IParser<string>>();
            string _;
            var innerRemaining = " \ntest\t\r " + ItemConstants.HiddenStatSuffix;
            innerMock.Setup(p => p.TryParse("stat", out innerRemaining, out _)).Returns(false);
            var sut = new ValidatingParser<string>(innerMock.Object);

            sut.TryParse("stat", out var remaining, out var _);

            Assert.AreEqual("test", remaining);
        }
    }
}